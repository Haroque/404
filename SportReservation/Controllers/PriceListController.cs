using Microsoft.AspNetCore.Mvc;
using SportReservation.Middlewares;
using SportReservation.Models;
using SportReservation.Services;

namespace SportReservation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PriceListController(PriceListService priceService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PriceListCreateDto dto)
    {
        if (HttpContext.LoggedUser().Role != UserRole.Admin)
        {
            return Forbid();
        }

        var price = await priceService.Create(dto);
        return Ok(price.ToDto());
    }
}