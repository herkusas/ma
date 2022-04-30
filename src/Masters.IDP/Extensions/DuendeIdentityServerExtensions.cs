using Masters.Storage.Contracts;
using Masters.Storage.Stores;

namespace Masters.IDP.Extensions;

public static class DuendeIdentityServerExtensions
{
    public static void AddConfigurationStore(this IIdentityServerBuilder builder, string cN)
    {
        builder.Services.AddSingleton(cN);
        builder.AddClientStore<ExtendedClientStore>();
        builder.AddResourceStore<ExtendedResourceStore>();
    }
}