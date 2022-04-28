using Masters.AdminAPI.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Masters.AdminAPI.Controllers;

[ApiController]
[Route("v1/resources")]
public class ResourcesController : ControllerBase
{
    
    [HttpPut]
    [Authorize(Policy = nameof(Policies.ManageResources))]
    public async Task<IActionResult> Save()
    {
        return await Task.FromResult<IActionResult>(Ok());
    }
}