using Duende.IdentityServer.Models;

namespace Ma.AdminAPI.Model
{
    public record ApiResourceRecord(
        string Name,
        ICollection<string> Scopes
    )
    {
        public ApiResource Map() => new(Name) {Scopes = Scopes};
    }
}