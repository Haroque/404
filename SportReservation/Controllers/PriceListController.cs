using Microsoft.AspNetCore.Mvc;
using SportReservation.Middlewares;
using SportReservation.Models;
using SportReservation.Services;

namespace SportReservation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PriceListController(PriceListService priceService) : ControllerBase
{
    private void EnsureAdmin()
    {
        if (HttpContext.LoggedUser().Role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException("Only admin can manage the prices"); //forbid nejel -> asi nejlepší ? 
        }
    }
    
    [HttpGet("{facilityTypeId:guid}")]
    public async Task<IActionResult> GetByFacilityType(Guid facilityTypeId, [FromQuery] bool onlyActive = false)
    {
        // admin vidí vše + filtr
        var prices = await priceService.GetByFacilityType(facilityTypeId, onlyActive);
        return Ok(prices.Select(p => p.ToDto()));
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PriceListCreateDto dto)
    {
        EnsureAdmin();
        var price = await priceService.Create(dto);
        return Ok(price.ToDto());
    }
    
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PriceListUpdateDto dto)
    {
        EnsureAdmin();
        var price = await priceService.Update(id, dto);
        return Ok(price.ToDto());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        EnsureAdmin();
        await priceService.Delete(id);
        return NoContent();
    }
}