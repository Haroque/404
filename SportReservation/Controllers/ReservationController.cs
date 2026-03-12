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
        if (r == null) return NotFound();
        return Ok(r);
    }
    
    // Nový endpoint: všechny rezervace přihlášeného uživatele, volitelně filtrovat aktivní/neaktivní
    [HttpGet]
    public async Task<IActionResult> GetForCurrentUser([FromQuery] bool? active)
    {
        var lu = HttpContext.LoggedUser();
        var list = await _svc.GetUserReservationsAsync(lu.Id, active);
        return Ok(list);
    }
    
    
    //DTO.UserId je nyní nepovinné.
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
    {
        var lu = HttpContext.LoggedUser();

        Guid effectiveUserId;
        if (dto.UserId == null || dto.UserId == Guid.Empty)
        {
            // pokud není user v body, použije se přihlášený uživatel
            effectiveUserId = lu.Id;
        }
        else
        {
            // jenom admin 
            if (lu.Role != UserRole.Admin)
                return Forbid();
            effectiveUserId = dto.UserId.Value;
        }

        
        var res = await _svc.CreateReservationAsync(effectiveUserId, dto.FacilityId, dto.StartAt, dto.EndAt);
        return CreatedAtAction(nameof(Get), new { id = res.Id }, res);
    }

    // nebere údaje o uživateli v query; používá přihlášeného uživatele
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var lu = HttpContext.LoggedUser();
        
        bool isAdmin = lu.Role == UserRole.Admin;
        
        await _svc.CancelReservationAsync(id, lu.Id, isAdmin);
        return NoContent();
    }
}

// DTO s nepovinným UserId (může být null => vytvoří se pro přihlášeného)
public record CreateReservationDto(Guid? UserId, Guid FacilityId, DateTime StartAt, DateTime EndAt); 