using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;

namespace Masters.Storage.Contracts;

public interface IExtendedClientStore : IClientStore
{
    Task Save(Client client);
    Task<bool> AllScopeExist(IEnumerable<string> name);
    Task<bool> Exist(Client client);
}