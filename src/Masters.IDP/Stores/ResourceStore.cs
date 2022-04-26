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
        var result = await connection.QueryAsync<ApiScope>(query, new {scopeNames});
        return result;
    }

    public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
    {
        return Task.FromResult<IEnumerable<ApiResource>>(new List<ApiResource>());
    }

    public Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames)
    {
        return Task.FromResult<IEnumerable<ApiResource>>(new List<ApiResource>());
    }

    public Task<Resources> GetAllResourcesAsync()
    {
        return Task.FromResult(new Resources());
    }
}