using Microsoft.AspNetCore.Mvc;
using SportReservation.Middlewares;
using SportReservation.Models;
using SportReservation.Services;

namespace SportReservation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FacilityController(FacilityService facilityService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FacilityCreateDto dto)
    {
        if (HttpContext.LoggedUser().Role != UserRole.Admin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "forbidden");
        }

        var facility = await facilityService.Create(dto);
        return Ok(facility.ToDto());
    }

    [HttpPost("Type")]
    public async Task<IActionResult> Create([FromBody] FacilityTypeCreateDto dto)
    {
        if (HttpContext.LoggedUser().Role != UserRole.Admin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "forbidden");
        }

        var type = await facilityService.CreateType(dto);
        return Ok(type.ToDto());
    }
}