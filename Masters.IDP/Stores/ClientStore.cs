using Dapper;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using IdentityModel;
using Npgsql;

namespace Masters.IDP.Stores;

public class ClientStore : IClientStore
{
    private readonly string _connectionString;

    public ClientStore(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Client> FindClientByIdAsync(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId)) return null;

        var tempDict = new Dictionary<string, Client>();

        const string query = "SELECT * FROM clients " +
                             "LEFT JOIN client_secrets ON client_secrets.client_id = clients.id " +
                             "LEFT JOIN client_scopes ON client_scopes.client_id = clients.id";

        await using var connection = new NpgsqlConnection(_connectionString);

        var client = await connection.QueryAsync<Client, ClientSecretRecord, ClientScopeRecord, Client>(
            query, (client, clientSecret, clientScopes) => MapClient(tempDict, client, clientSecret, clientScopes),
            new {clientId});
        return client.Distinct().FirstOrDefault();
    }

    private static Client MapClient(IDictionary<string, Client> tempDict, Client client, ClientSecretRecord clientSecret,
        ClientScopeRecord allowedScopes)
    {
        if (!tempDict.TryGetValue(client.ClientId, out var currentClient))
        {
            currentClient = client;
            currentClient = SetDefaultValues(currentClient);
            tempDict.Add(currentClient.ClientId, currentClient);
        }

        if (clientSecret != null)
        {
            currentClient.ClientSecrets.Add(clientSecret.Map());
        }

        if (allowedScopes != null)
        {
            currentClient.AllowedScopes.Add(allowedScopes.Scope);
        }
        
        return currentClient;
    }

    private static Client SetDefaultValues(Client client)
    {
        client.RequireClientSecret = true;
        client.LogoUri = null;
        client.RequireConsent = false;
        client.RequirePkce = false;
        client.AllowRememberConsent = true;
        client.AlwaysIncludeUserClaimsInIdToken = false;
        client.AllowPlainTextPkce = false;
        client.RequireRequestObject = false;
        client.AllowAccessTokensViaBrowser = false;
        client.BackChannelLogoutUri = null;
        client.BackChannelLogoutSessionRequired = true;
        client.IdentityTokenLifetime = 300;
        client.AllowedIdentityTokenSigningAlgorithms = null;
        client.AuthorizationCodeLifetime = 300;
        client.ConsentLifetime = null;
        client.AbsoluteRefreshTokenLifetime = 2592000;
        client.SlidingRefreshTokenLifetime = 1296000;
        client.RefreshTokenUsage = TokenUsage.OneTimeOnly;
        client.UpdateAccessTokenClaimsOnRefresh = false;
        client.RefreshTokenExpiration = TokenExpiration.Absolute;
        client.AccessTokenType = AccessTokenType.Jwt;
        client.EnableLocalLogin = true;
        client.IncludeJwtId = true;
        client.AlwaysSendClientClaims = true;
        client.ClientClaimsPrefix = string.Empty;
        client.PairWiseSubjectSalt = null;
        client.UserSsoLifetime = null;
        client.UserCodeType = null;
        client.DeviceCodeLifetime = 300;
        client.AllowedGrantTypes.Add(OidcConstants.GrantTypes.ClientCredentials);
        client.Claims = new List<ClientClaim>();
        client.ProtocolType = "oidc";
        client.Enabled = true;
        client.Description = null;
        client.FrontChannelLogoutUri = null;
        client.FrontChannelLogoutSessionRequired = true;
        client.AllowOfflineAccess = false;
        client.RequireClientSecret = true;
        return client;
    }
}

public class ClientSecretRecord
{
    public string Id { get; init; }
    public string Value { get; init; }
    public string Type { get; init; }
    
    public int ClientId { get; init; }
    public Secret Map()
    {
        return new Secret(Value, null, null)
        {
            Type = Type
        };
    }
};

public class ClientScopeRecord
{
    public string Id { get; init; }
    public string Scope { get; init; }
    
    public int ClientId { get; init; }
}