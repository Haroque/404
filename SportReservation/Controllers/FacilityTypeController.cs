using Microsoft.AspNetCore.Mvc;
using SportReservation.Middlewares;
using SportReservation.Models;
using SportReservation.Services;

namespace SportReservation.Controllers;

[ApiController]
[Route("api/Facility/Type")]
public class FacilityTypeController(FacilityService facilityService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!IsAdmin())
            return Forbid();

        var types = await facilityService.GetTypesAsync();
        return Ok(types);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FacilityTypeCreateDto body)
    {
        if (!IsAdmin())
            return Forbid();

        var type = await facilityService.CreateTypeAsync(body);
        return Ok(type);
    }


    [HttpPatch]
    public async Task<IActionResult> Patch([FromBody] FacilityTypePatchDto body)
    {
        if (!IsAdmin())
            return Forbid();

        var type = await facilityService.PatchTypeAsync(body);

        if (type == null)
            return NotFound();

        return Ok(type);
    }



    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!IsAdmin())
            return Forbid();

        var deleted = await facilityService.DeleteTypeAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    private bool IsAdmin()
    {
        return HttpContext.LoggedUser().Role == UserRole.Admin;
    }
}