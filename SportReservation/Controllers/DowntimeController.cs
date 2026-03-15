using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportReservation.Data;
using SportReservation.Middlewares;
using SportReservation.Models;

namespace SportReservation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DowntimeController(AppDbContext db) : ControllerBase
{
    [HttpGet("facility/{facilityId:guid}")]
    public async Task<IActionResult> GetForFacility(Guid facilityId)
    {
        var downtimes = await db.Downtimes
            .Where(d => d.FacilityId == facilityId && d.EndAt >= DateTime.UtcNow)
            .OrderBy(d => d.StartAt)
            .ToListAsync();

        return Ok(downtimes.Select(d => d.ToDto()));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDowntimeDto body)
    {
        if (HttpContext.LoggedUser().Role != UserRole.Admin)
        {
            return Forbid();
        }

        if (body.StartAt >= body.EndAt)
        {
            return BadRequest("start-after-end");
        }

        var facility = await db.Facilities.FindAsync(body.FacilityId);
        if (facility == null)
        {
            return NotFound("facility-not-found");
        }

        var downtime = new Downtime
        {
            Id = Guid.NewGuid(),
            FacilityId = body.FacilityId,
            StartAt = body.StartAt,
            EndAt = body.EndAt,
            Reason = body.Reason
        };

        await db.Downtimes.AddAsync(downtime);
        await db.SaveChangesAsync();

        return Ok(downtime.ToDto());
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDowntimeDto body)
    {
        if (HttpContext.LoggedUser().Role != UserRole.Admin)
        {
            return Forbid();
        }

        var downtime = await db.Downtimes.FindAsync(id);
        if (downtime == null)
        {
            return NotFound();
        }

        if (body.StartAt >= body.EndAt)
        {
            return BadRequest("start-after-end");
        }

        var facility = await db.Facilities.FindAsync(body.FacilityId);
        if (facility == null)
        {
            return NotFound("facility-not-found");
        }

        downtime.FacilityId = body.FacilityId;
        downtime.StartAt = body.StartAt;
        downtime.EndAt = body.EndAt;
        downtime.Reason = body.Reason;

        db.Downtimes.Update(downtime);
        await db.SaveChangesAsync();

        return Ok(downtime.ToDto());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (HttpContext.LoggedUser().Role != UserRole.Admin)
        {
            return Forbid();
        }

        var downtime = await db.Downtimes.FindAsync(id);
        if (downtime == null)
        {
            return NotFound();
        }

        db.Downtimes.Remove(downtime);
        await db.SaveChangesAsync();

        return NoContent();
    }
}

public record CreateDowntimeDto(
    Guid FacilityId,
    DateTime StartAt,
    DateTime EndAt,
    string Reason
);

public record UpdateDowntimeDto(
    Guid FacilityId,
    DateTime StartAt,
    DateTime EndAt,
    string Reason
);

public record DowntimeDto(
    Guid Id,
    Guid FacilityId,
    DateTime StartAt,
    DateTime EndAt,
    string Reason
);

public static class DowntimeDtoExtensions
{
    public static DowntimeDto ToDto(this Downtime downtime)
    {
        return new DowntimeDto(
            downtime.Id,
            downtime.FacilityId,
            downtime.StartAt,
            downtime.EndAt,
            downtime.Reason
        );
    }
}