using Duende.IdentityServer.Models;
using Masters.AdminAPI.Authorization;
using Masters.Storage.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Masters.AdminAPI.Controllers;

[ApiController]
[Route("v1/robots")]
public class RobotsController : ControllerBase
{
    private readonly IExtendedClientStore _extendedClientStore;

    public RobotsController(IExtendedClientStore extendedClientStore)
    {
        _extendedClientStore = extendedClientStore;
    }
    
    [HttpPut]
    [Authorize(Policy = nameof(Policies.ManageRobots))]
    public async Task<IActionResult> Save()
    {
        var robot = new Client
        {
            ClientId = "MegaClient",
            AllowedScopes = {},
            ClientSecrets =
            {
                new Secret("42BD490AC869ED79253392E00E31E0CCCEEF0724")
            }
        };

        if (!await _extendedClientStore.AllScopeExist(robot.AllowedScopes))
        {
            return await Task.FromResult<IActionResult>(BadRequest());
        }

        await _extendedClientStore.Save(robot);
        
        return await Task.FromResult<IActionResult>(Ok());
    }
}