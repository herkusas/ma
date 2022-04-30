using Duende.IdentityServer.Models;
using Masters.AdminAPI.Authorization;
using Masters.Storage.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Masters.AdminAPI.Controllers;

[ApiController]
[Route("v1/resources")]
public class ResourcesController : ControllerBase
{
    private readonly IExtendedResourceStore _resourceStore;

    public ResourcesController(IExtendedResourceStore resourceStore)
    {
        _resourceStore = resourceStore;
    }
    
    [HttpPut]
    [Authorize(Policy = nameof(Policies.ManageResources))]
    public async Task<IActionResult> Save()
    {
        var test = new ApiResource("https://localhost:20000")
        {
            Scopes = new List<string>{"urn:random:scope"}
        };

        await _resourceStore.Save(test);
        
        return await Task.FromResult<IActionResult>(Ok());
    }
}