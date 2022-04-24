using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Masters.IDP
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
            new IdentityResources.OpenId()
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new[]
                { new ApiScope("test_scope")};

        public static IEnumerable<Client> Clients =>
            new[]
                { new Client
                {
                    ClientId = "test_client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = { "test_scope" },
                    ClientSecrets = 
                    {
                        new Secret("42bd490ac869ed79253392e00e31e0ccceef0724")
                        {
                            Type = IdentityServerConstants.SecretTypes.X509CertificateThumbprint
                        },
                    }
                }};
    }
}