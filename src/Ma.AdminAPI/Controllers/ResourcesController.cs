using Ma.AdminAPI.Authorization;
using Ma.Contracts;
using Ma.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ma.AdminAPI.Controllers;

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
    public async Task<IActionResult> Save(ApiResourceRecord record)
    {
        var apiResource = record.Map();

        var exist = await _resourceStore.Exist(apiResource);

        await _resourceStore.Save(apiResource);

        return exist ? Ok(record) : CreatedAtAction(nameof(Save), new {Id = record.Name}, record);
    }
}
