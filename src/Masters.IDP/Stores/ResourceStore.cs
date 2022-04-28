using Dapper;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Npgsql;

namespace Masters.IDP.Stores;

public class ResourceStore : IResourceStore
{
    private readonly string _connectionString;

    public ResourceStore(string connectionString)
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
                             "LEFT JOIN api_scopes ON api_scopes.id = api_resource_scopes.id " +
                             "WHERE api_scopes.name = ANY(@scopeNames)";
        
        var tempDict = new Dictionary<string, ApiResource>();
        
        await using var connection = new NpgsqlConnection(_connectionString);
        
        var apiResources = await connection.QueryAsync<ApiResource, string, ApiResource>(
            query, (apiResource, apiScope) => MapResource(tempDict,apiResource,apiScope),
            new {scopeNames = scopeNames.ToArray()}, splitOn: "api_scope");
        
        return apiResources.Distinct();
    }

    public async Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames)
    {
        const string query = "SELECT api_resources.*, api_scopes.name as api_scope FROM api_resources " +
                             "LEFT JOIN api_resource_scopes ON api_resource_scopes.api_resource_id = api_resources.id " +
                             "LEFT JOIN api_scopes ON api_scopes.id = api_resource_scopes.id " +
                             "WHERE api_resources.name = ANY(@apiResourceNames)";
        
        var tempDict = new Dictionary<string, ApiResource>();
        
        await using var connection = new NpgsqlConnection(_connectionString);
        
        var apiResources = await connection.QueryAsync<ApiResource, string, ApiResource>(
            query, (apiResource, apiScope) => MapResource(tempDict,apiResource,apiScope),
            new {apiResourceNames = apiResourceNames.ToArray()}, splitOn: "api_scope");
        
        return apiResources.Distinct();
    }

    public Task<Resources> GetAllResourcesAsync()
    {
        return Task.FromResult(new Resources());
    }
    
    private static ApiResource MapResource(IDictionary<string, ApiResource> tempDict, ApiResource apiResource, string apiScope)
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
}