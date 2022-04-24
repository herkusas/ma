using Duende.IdentityServer.Configuration;
using Masters.IDP.Stores;

namespace Masters.IDP;

public static class DuendeIdentityServerExtensions
{
    public static IIdentityServerBuilder AddConfigurationStore(this IIdentityServerBuilder builder, string cN)
    {
        builder.Services.AddSingleton(cN);
        builder.AddClientStore<ClientStore>();
        return builder;
    }
}