using System.Data;
using Dapper;
using Duende.IdentityServer.Models;
using Ma.Contracts;
using Npgsql;

namespace Ma.Shared.Storage.Stores;

// ReSharper disable once ClassNeverInstantiated.Global
public class ExtendedClientStore : IExtendedClientStore
{
    private readonly string _connectionString;

    public ExtendedClientStore(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<bool> Exist(Client client)
    {
        const string Query = "SELECT TRUE FROM clients WHERE clients.client_id = @clientId";
        await using var connection = new NpgsqlConnection(_connectionString);
        var result = await connection.ExecuteScalarAsync<bool>(Query, new {clientId = client.ClientId});
        return result;
    }

    public async Task<Client?> FindClientByIdAsync(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return null;
        }

        var tempDict = new Dictionary<string, Client>();

        const string Query =
            "SELECT clients.*, client_secrets.type, client_secrets.value, api_scopes.name as allowed_scope FROM clients " +
            "LEFT JOIN client_secrets ON client_secrets.client_id = clients.id " +
            "LEFT JOIN client_scopes ON client_scopes.client_id = clients.id " +
            "LEFT JOIN api_scopes ON api_scopes.id = client_scopes.scope_id  " +
            "WHERE clients.client_id = @clientId";

        await using var connection = new NpgsqlConnection(_connectionString);

        IEnumerable<Client?> client = await connection.QueryAsync<Client, Secret, string, Client>(
            Query, (client, clientSecret, allowedScope) => MapClient(tempDict, client, clientSecret, allowedScope),
            new {clientId}, splitOn: "type,allowed_scope");
        return client.Distinct().FirstOrDefault();
    }

    public async Task Save(Client client)
    {
        var existingClient = await FindClientByIdAsync(client.ClientId);

        if (existingClient != null)
        {
            await Update(client, existingClient);
        }
        else
        {
            await Insert(client);
        }
    }


    public async Task<bool> AllScopeExist(IEnumerable<string> scopes)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<bool>(
            @"SELECT true FROM api_scopes WHERE api_scopes.name = ALL(@scopes)", new {scopes = scopes.ToArray()});
    }

    private static Client MapClient(IDictionary<string, Client> tempDict, Client client, Secret? clientSecret,
        string? allowedScope)
    {
        if (!tempDict.TryGetValue(client.ClientId, out var currentClient))
        {
            client.AllowedGrantTypes.Add(GrantType.ClientCredentials);
            currentClient = client;
            tempDict.Add(currentClient.ClientId, currentClient);
        }

        if (clientSecret != null)
        {
            currentClient.ClientSecrets.Add(clientSecret);
        }

        if (allowedScope != null)
        {
            currentClient.AllowedScopes.Add(allowedScope);
        }

        return currentClient;
    }

    private async Task Insert(Client client)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var toAddScopes = client.AllowedScopes;
            var toAddApiScopesIds = new List<int>();
            foreach (var scope in toAddScopes)
            {
                var existingScopeId = await FindApiScopeIdByNameAsync(connection, transaction, scope);
                if (existingScopeId != null)
                {
                    toAddApiScopesIds.Add((int)existingScopeId);
                }
                else
                {
                    throw new Exception("Register scope first");
                }
            }

            var newClientId = await InsertClient(connection, transaction, client);
            foreach (var scopeId in toAddApiScopesIds)
            {
                await InsertClientScopes(connection, transaction, newClientId, scopeId);
            }

            foreach (var secret in client.ClientSecrets)
            {
                await InsertClientSecrets(connection, transaction, newClientId, secret.Value);
            }

            await transaction.CommitAsync();
            await connection.CloseAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            await connection.CloseAsync();
            throw;
        }
    }

    private async Task Update(Client client, Client existingClient)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var thereWasChanges = false;

            var toAddScopes =
                client.AllowedScopes.Where(scope =>
                        !existingClient.AllowedScopes.Contains(scope, StringComparer.InvariantCultureIgnoreCase))
                    .Distinct();
            var toRemoveScopes =
                existingClient.AllowedScopes.Where(scope =>
                    !client.AllowedScopes.Contains(scope, StringComparer.InvariantCultureIgnoreCase)).Distinct();

            var toAddSecretsValue = client.ClientSecrets.Select(secret => secret.Value).ToList();

            var existingClientSecretsValue = existingClient.ClientSecrets.Select(secret => secret.Value).ToList();

            var toAddSecrets =
                toAddSecretsValue.Where(secret =>
                    !existingClientSecretsValue.Contains(secret, StringComparer.InvariantCultureIgnoreCase)).Distinct();
            var toRemoveSecrets =
                existingClientSecretsValue.Where(secret =>
                    !toAddSecretsValue.Contains(secret, StringComparer.InvariantCultureIgnoreCase)).Distinct();

            var toAddApiScopesIds = new List<int>();
            foreach (var scope in toAddScopes)
            {
                var existingScopeId = await FindApiScopeIdByNameAsync(connection, transaction, scope);
                if (existingScopeId != null)
                {
                    toAddApiScopesIds.Add((int)existingScopeId);
                }
                else
                {
                    throw new Exception("Register scope first");
                }
            }

            var toBeRemovedScopesIds = new List<int>();
            foreach (var scope in toRemoveScopes)
            {
                var existingScopeId = await FindApiScopeIdByNameAsync(connection, transaction, scope);
                if (existingScopeId != null)
                {
                    toBeRemovedScopesIds.Add((int)existingScopeId);
                }
            }

            var existingClientId =
                await GetExistingClientId(connection, transaction, existingClient.ClientId);

            foreach (var scopeId in toAddApiScopesIds)
            {
                await InsertClientScopes(connection, transaction, existingClientId, scopeId);
                thereWasChanges = true;
            }

            foreach (var toBeRemovedScopeId in toBeRemovedScopesIds)
            {
                await RemoveClientsScopes(connection, transaction, existingClientId, toBeRemovedScopeId);
                thereWasChanges = true;
            }

            foreach (var secret in toAddSecrets)
            {
                await InsertClientSecrets(connection, transaction, existingClientId, secret);
                thereWasChanges = true;
            }

            foreach (var secret in toRemoveSecrets)
            {
                await RemoveSecrets(connection, transaction, existingClientId, secret);
                thereWasChanges = true;
            }

            if (thereWasChanges)
            {
                await UpdateClient(connection, transaction, existingClientId, client.ClientName,
                    client.AccessTokenLifetime);
            }

            await transaction.CommitAsync();
            await connection.CloseAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            await connection.CloseAsync();
            throw;
        }
    }

    private static Task<int?> FindApiScopeIdByNameAsync(IDbConnection connection, IDbTransaction transaction,
        string name)
    {
        return connection.ExecuteScalarAsync<int?>(@"SELECT id FROM api_scopes WHERE api_scopes.name =@name",
            new {name},
            transaction);
    }

    private static Task<int> InsertClient(IDbConnection connection, IDbTransaction transaction,
        Client client)
    {
        return connection.ExecuteScalarAsync<int>(
            @"INSERT INTO clients(client_id, client_name, access_token_lifetime) VALUES(@ClientId, @ClientName, @AccessTokenLifetime) RETURNING id",
            new {client.ClientId, client.ClientName, client.AccessTokenLifetime},
            transaction);
    }

    private static Task InsertClientScopes(IDbConnection connection, IDbTransaction transaction,
        int clientId, int scopeId)
    {
        return connection.ExecuteAsync(
            @"INSERT INTO client_scopes(client_id, scope_id) VALUES (@clientId,@scopeId)",
            new {clientId, scopeId}, transaction);
    }

    private static Task InsertClientSecrets(IDbConnection connection, IDbTransaction transaction,
        int clientId, string value)
    {
        return connection.ExecuteAsync(
            @"INSERT INTO client_secrets(client_id, value, type)VALUES (@clientId,@value,@type)",
            new {clientId, value, Type = "X509Thumbprint"}, transaction);
    }

    private static Task<int> GetExistingClientId(IDbConnection connection, IDbTransaction transaction,
        string clientId)
    {
        return connection.ExecuteScalarAsync<int>(@"SELECT id FROM clients WHERE clients.client_id =@clientId",
            new {clientId},
            transaction);
    }

    private static Task RemoveClientsScopes(IDbConnection connection, IDbTransaction transaction,
        int clientId, int scopeId)
    {
        return connection.ExecuteAsync(
            @"DELETE FROM client_scopes WHERE client_scopes.client_id = @clientId AND client_scopes.scope_id = @scopeId",
            new {clientId, scopeId}, transaction);
    }

    private static Task RemoveSecrets(IDbConnection connection, IDbTransaction transaction,
        int clientId, string value)
    {
        return connection.ExecuteAsync(
            @"DELETE FROM client_secrets WHERE client_secrets.client_id = @clientId AND client_secrets.value = @value",
            new {clientId, value}, transaction);
    }

    private static Task UpdateClient(IDbConnection connection, IDbTransaction transaction,
        int id, string name, int clientAccessTokenLifetime)
    {
        return connection.ExecuteAsync(
            @"UPDATE clients SET updated = current_timestamp, client_name = @name, access_token_lifetime = @clientAccessTokenLifetime WHERE clients.id = @id",
            new {id, name, clientAccessTokenLifetime},
            transaction);
    }
}
