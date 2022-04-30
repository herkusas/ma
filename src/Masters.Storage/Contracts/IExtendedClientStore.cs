using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;

namespace Masters.Storage.Contracts;

public interface IExtendedClientStore : IClientStore
{
    Task Save(Client client);
    Task<bool> Exists(string clientId);
}