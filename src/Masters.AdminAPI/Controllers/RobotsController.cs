using Duende.IdentityServer.Models;
using Masters.AdminAPI.Authorization;
using Masters.AdminAPI.Model;
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
    public async Task<IActionResult> Save(RobotRecord request)
    {
        var client = request.Map();

        var exist = await _extendedClientStore.Exist(client);

        if (!await _extendedClientStore.AllScopeExist(client.AllowedScopes))
        {
            return BadRequest(new {message = "Check if all scopes are registered within any resource"});
        }

        await _extendedClientStore.Save(client);
        
        return exist ? Ok(request) : 
            CreatedAtAction(nameof(Save), new {Id = request.RobotId}, request);
    }
}