using Masters.AdminAPI.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Masters.AdminAPI.Controllers;

[ApiController]
[Route("v1/robots")]
public class RobotsController : ControllerBase
{
    
    [HttpPut]
    [Authorize(Policy = nameof(Policies.ManageRobots))]
    public async Task<IActionResult> Save()
    {
        return await Task.FromResult<IActionResult>(Ok());
    }
}