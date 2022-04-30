using Dapper;
using Duende.IdentityServer.Models;
using Masters.Storage.Contracts;
using Npgsql;

namespace Masters.Storage.Stores;

// ReSharper disable once ClassNeverInstantiated.Global
public class ExtendedClientStore : IExtendedClientStore
{
    private readonly string _connectionString;

    public ExtendedClientStore(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Client?> FindClientByIdAsync(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId)) return null;

        var tempDict = new Dictionary<string, Client>();

        const string query = "SELECT clients.*, client_secrets.type, client_secrets.value, api_scopes.name as allowed_scope, client_grants.type as allowed_grant_types FROM clients " +
                             "LEFT JOIN client_secrets ON client_secrets.client_id = clients.id " +
                             "LEFT JOIN client_scopes ON client_scopes.client_id = clients.id " +
                             "LEFT JOIN api_scopes ON api_scopes.id = client_scopes.id  " +
                             "LEFT JOIN client_grants ON client_grants.client_id = clients.id " +
                             "WHERE clients.client_id = @clientId";

        await using var connection = new NpgsqlConnection(_connectionString);

        IEnumerable<Client?> client = await connection.QueryAsync<Client, Secret, string, string, Client>(
            query, (client, clientSecret, allowedScope, allowedGrantType) => MapClient(tempDict, client, clientSecret, allowedScope, allowedGrantType),
            new {clientId}, splitOn: "type,allowed_scope,allowed_grant_types");
        return client.Distinct().FirstOrDefault();
    }

    private static Client MapClient(IDictionary<string, Client> tempDict, Client client, Secret? clientSecret,
        string? allowedScope, string? allowedGrantType)
    {
        if (!tempDict.TryGetValue(client.ClientId, out var currentClient))
        {
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
        
        if (allowedGrantType != null)
        {
            currentClient.AllowedGrantTypes.Add(allowedGrantType);
        }
        
        return currentClient;
    }

    public Task Save(Client client)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> Exists(string clientId)
    {
        const string query = "SELECT true FROM clients WHERE clients.client_id = @clientId";
        
        await using var connection = new NpgsqlConnection(_connectionString);

        return await connection.ExecuteScalarAsync<bool>(query, new {clientId});
    }
}