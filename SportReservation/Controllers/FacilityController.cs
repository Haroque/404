using Microsoft.AspNetCore.Mvc;
using SportReservation.Middlewares;
using SportReservation.Models;
using SportReservation.Services;

namespace SportReservation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FacilityController(FacilityService facilityService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery(Name = "page")] int page = 1,
        [FromQuery(Name = "per_page")] int perPage = 10)
    {
        var facilities = await facilityService.GetPagedAsync(page, perPage);
        return Ok(facilities);
    }


    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        if (!IsAdmin())
            return Forbid();

        var facility = await facilityService.GetAsync(id);

        if (facility == null)
            return NotFound();

        return Ok(facility.ToComplexDto());
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FacilityCreateDto body)
    {
        if (!IsAdmin())
            return Forbid();

        var facility = await facilityService.CreateAsync(body);

        if (facility == null)
            return NotFound();

        return Ok(facility);
    }

    [HttpPatch]
    public async Task<IActionResult> Patch([FromBody] FacilityPatchDto body)
    {
        if (!IsAdmin())
            return Forbid();

        var facility = await facilityService.PatchAsync(body);

        if (facility == null)
            return NotFound();

        return Ok(facility);
    }




    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!IsAdmin())
            return Forbid();

        var deleted = await facilityService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    private bool IsAdmin()
    {
        return HttpContext.LoggedUser().Role == UserRole.Admin;
    }
}