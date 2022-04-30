using System.Data;
using Dapper;
using Duende.IdentityServer.Models;
using Masters.Storage.Contracts;
using Npgsql;

namespace Masters.Storage.Stores;

// ReSharper disable once ClassNeverInstantiated.Global
public class ExtendedResourceStore : IExtendedResourceStore
{
    private readonly string _connectionString;

    public ExtendedResourceStore(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
    {
        return Task.FromResult<IEnumerable<IdentityResource>>(new List<IdentityResource>());
    }

    public async Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
    {
        const string query = "SELECT * FROM api_scopes WHERE api_scopes.name = ANY(@scopeNames)";
        await using var connection = new NpgsqlConnection(_connectionString);
        var result = await connection.QueryAsync<ApiScope>(query, new {scopeNames = scopeNames.ToArray()});
        return result.Distinct();
    }

    public async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
    {
        const string query = "SELECT api_resources.*, api_scopes.name as api_scope FROM api_resources " +
                             "LEFT JOIN api_resource_scopes ON api_resource_scopes.api_resource_id = api_resources.id " +
                             "LEFT JOIN api_scopes ON api_scopes.id = api_resource_scopes.scope_id " +
                             "WHERE api_scopes.name = ANY(@scopeNames)";

        var tempDict = new Dictionary<string, ApiResource>();

        await using var connection = new NpgsqlConnection(_connectionString);

        var apiResources = await connection.QueryAsync<ApiResource, string, ApiResource>(
            query, (apiResource, apiScope) => MapResource(tempDict, apiResource, apiScope),
            new {scopeNames = scopeNames.ToArray()}, splitOn: "api_scope");

        return apiResources.Distinct();
    }

    public async Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames)
    {
        const string query = "SELECT api_resources.*, api_scopes.name as api_scope FROM api_resources " +
                             "LEFT JOIN api_resource_scopes ON api_resource_scopes.api_resource_id = api_resources.id " +
                             "LEFT JOIN api_scopes ON api_scopes.id = api_resource_scopes.scope_id " +
                             "WHERE api_resources.name = ANY(@apiResourceNames)";

        var tempDict = new Dictionary<string, ApiResource>();

        await using var connection = new NpgsqlConnection(_connectionString);

        var apiResources = await connection.QueryAsync<ApiResource, string, ApiResource>(
            query, (apiResource, apiScope) => MapResource(tempDict, apiResource, apiScope),
            new {apiResourceNames = apiResourceNames.ToArray()}, splitOn: "api_scope");

        return apiResources.Distinct();
    }

    public Task<Resources> GetAllResourcesAsync()
    {
        return Task.FromResult(new Resources());
    }

    private static ApiResource MapResource(IDictionary<string, ApiResource> tempDict, ApiResource apiResource,
        string? apiScope)
    {
        if (!tempDict.TryGetValue(apiResource.Name, out var currentResource))
        {
            currentResource = apiResource;
            tempDict.Add(currentResource.Name, currentResource);
        }

        if (apiScope != null)
        {
            currentResource.Scopes.Add(apiScope);
        }

        return currentResource;
    }

    public async Task Save(ApiResource apiResource)
    {
        var existingResources = await FindApiResourcesByNameAsync(new List<string> {apiResource.Name});
        var existingResource = existingResources.FirstOrDefault();

        if (existingResource != null)
        {
            await Update(apiResource, existingResource);
        }
        else
        {
            await Insert(apiResource);
        }
    }

    private async Task Update(ApiResource apiResource, ApiResource existingResource)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var thereWasChanges = false;
            
            var toAddScopesFromApiResource =
                apiResource.Scopes.Where(scope => !existingResource.Scopes.Contains(scope, StringComparer.InvariantCultureIgnoreCase)).Distinct();
            var toRemoveScopesFromApiResource =
                existingResource.Scopes.Where(scope => !apiResource.Scopes.Contains(scope, StringComparer.InvariantCultureIgnoreCase)).Distinct();
            
            var existingApiScopesIds = new List<int>();
            var toAddApiScopes = new List<string>();
            foreach (var scope in toAddScopesFromApiResource)
            {
                var existingScopeId = await FindApiScopeIdByNameAsync(connection, transaction, scope);
                if (existingScopeId != null)
                {
                    existingApiScopesIds.Add((int) existingScopeId);
                }
                else
                {
                    toAddApiScopes.Add(scope);
                }
            }

            var newScopesIds = new List<int>();
            foreach (var scope in toAddApiScopes)
            {
                var scopeId = await InsertScope(connection, transaction, scope);
                newScopesIds.Add(scopeId);
            }

            var existingResourceId =
                await GetExistingApiResourceIdByName(connection, transaction, existingResource.Name);

            foreach (var newScopesId in newScopesIds)
            {
                await InsertApiResourceScopes(connection, transaction, existingResourceId, newScopesId);
                thereWasChanges = true;
            }

            foreach (var existingScopeId in existingApiScopesIds)
            {
                await InsertApiResourceScopes(connection, transaction, existingResourceId, existingScopeId);
                thereWasChanges = true;
            }

            var toBeRemovedScopesIds = new List<int>();
            foreach (var scope in toRemoveScopesFromApiResource)
            {
                var existingScopeId = await FindApiScopeIdByNameAsync(connection, transaction, scope);
                if (existingScopeId != null)
                {
                    toBeRemovedScopesIds.Add((int) existingScopeId);
                }
            }

            foreach (var toBeRemovedScopeId in toBeRemovedScopesIds)
            {
                await RemoveApiResourceScopes(connection, transaction, existingResourceId, toBeRemovedScopeId);
                thereWasChanges = true;
                if (await CheckIfAnyResourceHasThisScope(connection, transaction, toBeRemovedScopeId)) continue;
                await DeleteApiScope(connection, transaction, toBeRemovedScopeId);
                thereWasChanges = true;
            }
            
            if (thereWasChanges)
                await UpdateApiResource(connection,transaction,existingResourceId);
            
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

    private async Task Insert(ApiResource apiResource)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var toAddScopesFromApiResource = apiResource.Scopes;
            var existingApiScopesIds = new List<int>();
            var toAddApiScopes = new List<string>();
            foreach (var scope in toAddScopesFromApiResource)
            {
                var existingScopeId = await FindApiScopeIdByNameAsync(connection, transaction, scope);
                if (existingScopeId != null)
                {
                    existingApiScopesIds.Add((int) existingScopeId);
                }
                else
                {
                    toAddApiScopes.Add(scope);
                }
            }

            var newScopesIds = new List<int>();
            foreach (var scope in toAddApiScopes)
            {
                var scopeId = await InsertScope(connection, transaction, scope);
                newScopesIds.Add(scopeId);
            }

            var newResourceId = await InsertApiResource(connection, transaction, apiResource.Name);

            foreach (var newScopesId in newScopesIds)
            {
                await InsertApiResourceScopes(connection, transaction, newResourceId, newScopesId);
            }

            foreach (var existingScopeId in existingApiScopesIds)
            {
                await InsertApiResourceScopes(connection, transaction, newResourceId, existingScopeId);
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
        => connection.ExecuteScalarAsync<int?>(@"SELECT id FROM api_scopes WHERE api_scopes.name =@name", new {name},
            transaction);


    private static Task<int> InsertScope(IDbConnection connection, IDbTransaction transaction, string scope) =>
        connection.ExecuteScalarAsync<int>(@"INSERT INTO api_scopes(name) VALUES(@scope) RETURNING id", new {scope},
            transaction);

    private static Task<int> InsertApiResource(IDbConnection connection, IDbTransaction transaction,
        string name) =>
        connection.ExecuteScalarAsync<int>(@"INSERT INTO api_resources(name) VALUES(@name) RETURNING id", new {name},
            transaction);

    private static Task InsertApiResourceScopes(IDbConnection connection, IDbTransaction transaction,
        int resourceId, int scopeId) =>
        connection.ExecuteAsync(
            @"INSERT INTO api_resource_scopes(api_resource_id, scope_id) VALUES (@resourceId,@scopeId)",
            new {resourceId, scopeId}, transaction);

    private static Task RemoveApiResourceScopes(IDbConnection connection, IDbTransaction transaction,
        int resourceId, int scopeId) =>
        connection.ExecuteAsync(
            @"DELETE FROM api_resource_scopes WHERE api_resource_scopes.api_resource_id = @resourceId AND api_resource_scopes.scope_id = @scopeId",
            new {resourceId, scopeId}, transaction);

    private static Task<int> GetExistingApiResourceIdByName(IDbConnection connection, IDbTransaction transaction,
        string name) =>
        connection.ExecuteScalarAsync<int>(@"SELECT id FROM api_resources WHERE api_resources.name =@name", new {name},
            transaction);

    private static Task<bool> CheckIfAnyResourceHasThisScope(IDbConnection connection, IDbTransaction transaction,
        int id) =>
        connection.ExecuteScalarAsync<bool>(
            @"SELECT true FROM api_resource_scopes WHERE api_resource_scopes.scope_id = @id", new {id}, transaction);

    private static Task DeleteApiScope(IDbConnection connection, IDbTransaction transaction,
        int id) =>
        connection.ExecuteAsync(@"DELETE FROM api_scopes WHERE api_scopes.id = @id", new {id}, transaction);
    
    private static Task UpdateApiResource(IDbConnection connection, IDbTransaction transaction,
        int id) =>
        connection.ExecuteAsync(@"UPDATE api_resources SET updated = current_timestamp WHERE api_resources.id = @id", new {id}, transaction);
}