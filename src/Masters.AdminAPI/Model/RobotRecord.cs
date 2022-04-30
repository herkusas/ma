using Duende.IdentityServer.Models;

namespace Masters.AdminAPI.Model;

public record RobotRecord(
    string RobotId,
    string Name,
    ICollection<string> Scopes,
    ICollection<string> Thumbprints
)
{
    public Client Map()
    {
        var secrets = Thumbprints.Select(secret => new Secret(secret)).ToList();

        return new Client
        {
            ClientId = RobotId,
            ClientName = Name,
            AllowedScopes = Scopes,
            ClientSecrets = secrets
        };
    }
}