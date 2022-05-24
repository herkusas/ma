using Duende.IdentityServer.Models;

namespace Ma.Model;

public record ApiResourceRecord(
    string Name,
    ICollection<string> Scopes
)
{
    public ApiResource Map()
    {
        return new ApiResource(Name) {Scopes = Scopes};
    }
}
