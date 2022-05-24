using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;

namespace Ma.Contracts;

public interface IExtendedResourceStore : IResourceStore
{
    Task Save(ApiResource apiResource);
    Task<bool> Exist(ApiResource apiResource);
}
