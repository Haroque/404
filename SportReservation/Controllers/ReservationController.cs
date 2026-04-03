using Microsoft.AspNetCore.Mvc;
using SportReservation.Services;
using SportReservation.Middlewares;
using SportReservation.Models;

namespace SportReservation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly ReservationService _svc;

    public ReservationsController(ReservationService svc)
    {
        _svc = svc;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var r = await _svc.GetReservationAsync(id);

        if (r == null)
            return NotFound();

        return Ok(r.ToDto());
    }

    [HttpGet]
    public async Task<IActionResult> GetFiltered(
        [FromQuery(Name = "user_id")] Guid? userId,
        [FromQuery(Name = "facility_id")] Guid? facilityId,
        [FromQuery] bool? active)
    {
        var loggedUser = HttpContext.LoggedUser();

        Guid? effectiveUserId = userId;

        if (loggedUser.Role != UserRole.Admin)
        {
            if (userId.HasValue && userId.Value != loggedUser.Id)
                return StatusCode(StatusCodes.Status403Forbidden, "forbidden");

            effectiveUserId = loggedUser.Id;
        }

        var list = await _svc.GetReservationsAsync(effectiveUserId, facilityId, active);
        var dtos = list.Select(r => r.ToDto());

        return Ok(dtos);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
    {
        var lu = HttpContext.LoggedUser();

        Guid effectiveUserId;

        if (dto.UserId == null || dto.UserId == Guid.Empty)
        {
            effectiveUserId = lu.Id;
        }
        else
        {
            if (lu.Role != UserRole.Admin)
                return StatusCode(StatusCodes.Status403Forbidden, "forbidden");

            effectiveUserId = dto.UserId.Value;
        }

        var res = await _svc.CreateReservationAsync(
            effectiveUserId,
            dto.FacilityId,
            dto.StartAt,
            dto.EndAt
        );

        return CreatedAtAction(nameof(Get), new { id = res.Id }, res.ToDto());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await _svc.CancelReservationAsync(id, HttpContext.LoggedUser());
        return NoContent();
    }
}

public record CreateReservationDto(Guid? UserId, Guid FacilityId, DateTime StartAt, DateTime EndAt);