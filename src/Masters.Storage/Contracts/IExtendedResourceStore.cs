using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;

namespace Masters.Storage.Contracts;

public interface IExtendedResourceStore : IResourceStore
{
    Task Save(ApiResource apiResource);
}