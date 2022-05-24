using Masters.Storage.Stores;

namespace Ma.IDP.Extensions
{
    public static class DuendeIdentityServerExtensions
    {
        public static void AddConfigurationStore(this IIdentityServerBuilder builder, string cN)
        {
            builder.Services.AddSingleton(cN);
            builder.AddClientStore<ExtendedClientStore>();
            builder.AddResourceStore<ExtendedResourceStore>();
        }
    }
}