using Ma.AdminAPI.Authorization;
using Ma.Contracts;
using Ma.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ma.AdminAPI.Controllers;

[ApiController]
[Route("v1/robots")]
public class RobotsController : ControllerBase
{
    private readonly IExtendedClientStore _clientStore;

    public RobotsController(IExtendedClientStore clientStore)
    {
        _clientStore = clientStore;
    }

    [HttpPut]
    [Authorize(Policy = nameof(Policies.ManageRobots))]
    public async Task<IActionResult> Save(RobotRecord request)
    {
        var client = request.Map();

        var exist = await _clientStore.Exist(client);

        if (!await _clientStore.AllScopeExist(client.AllowedScopes))
        {
            return BadRequest(new {message = "Check if all scopes are registered within any resource"});
        }

        await _clientStore.Save(client);

        return exist ? Ok(request) : CreatedAtAction(nameof(Save), new {Id = request.RobotId}, request);
    }
}
