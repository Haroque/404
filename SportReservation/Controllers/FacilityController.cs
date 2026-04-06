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
        [FromQuery(Name = "per_page")] int perPage = 10,
        [FromQuery(Name = "type_id")] Guid? typeId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        // from a to musí být buď oba, nebo ani jeden
        if ((from.HasValue && !to.HasValue) || (!from.HasValue && to.HasValue))
            return BadRequest("Both 'from' and 'to' must be provided together.");

        // kontrola správného časového rozsahu
        if (from.HasValue && to.HasValue && from.Value >= to.Value)
            return BadRequest("'from' must be earlier than 'to'.");

        var facilities = await facilityService.GetPagedAsync(page, perPage, typeId, from, to);
        return Ok(facilities);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        if (!IsAdmin())
            return StatusCode(StatusCodes.Status403Forbidden, "forbidden");

        var facility = await facilityService.GetAsync(id);

        if (facility == null)
            return NotFound();

        return Ok(facility.ToComplexDto());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FacilityCreateDto body)
    {
        if (!IsAdmin())
            return StatusCode(StatusCodes.Status403Forbidden, "forbidden");

        var facility = await facilityService.CreateAsync(body);

        if (facility == null)
            return NotFound();

        return Ok(facility);
    }

    [HttpPatch]
    public async Task<IActionResult> Patch([FromBody] FacilityPatchDto body)
    {
        if (!IsAdmin())
            return StatusCode(StatusCodes.Status403Forbidden, "forbidden");

        var facility = await facilityService.PatchAsync(body);

        if (facility == null)
            return NotFound();

        return Ok(facility);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!IsAdmin())
            return StatusCode(StatusCodes.Status403Forbidden, "forbidden");

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