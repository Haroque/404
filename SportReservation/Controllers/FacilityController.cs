using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportReservation.Data;
using SportReservation.Middlewares;
using SportReservation.Models;
using SportReservation.Services;

namespace SportReservation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FacilityController(AppDbContext db, FacilityService facilityService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous] // chcou at to je public
    public async Task<IActionResult> GetAll([FromQuery] int page)
    {
        // page dodelat
        
        return Ok(await db.Facilities
            .Include(x => x.Type)
            .Include(x => x.Type.PriceLists)
            .Include(x => x.Downtimes)
            .Include(x => x.Reservations)
            .Select(x => x.ToComplexDto())
            .ToListAsync()
        );
    }

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
    public async Task<IActionResult> CreateType([FromBody] FacilityTypeCreateDto dto)
    {
        if (HttpContext.LoggedUser().Role != UserRole.Admin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "forbidden");
        }

        var type = await facilityService.CreateType(dto);
        return Ok(type.ToDto());
    }
}