using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportReservation.Data;

namespace SportReservation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OtherController(AppDbContext db) : ControllerBase
{
    [HttpGet("Dashboard")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var lastWeek = DateTime.UtcNow.AddDays(-7);

        var recentUsers = await db.Users
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .Select(u => new { u.FullName, u.Email, u.CreatedAt })
            .ToListAsync();

        var popularTypes = await db.Reservations
            .Include(r => r.Facility)
            .ThenInclude(f => f.Type)
            .GroupBy(r => r.Facility.Type.Name)
            .Select(g => new { TypeName = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        var reservationsLastWeek = await db.Reservations
            .Where(r => r.CreatedAt >= lastWeek)
            .GroupBy(r => r.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        var last7DaysStats = Enumerable.Range(0, 7)
            .Select(i => lastWeek.AddDays(i).Date)
            .Select(date => new
            {
                Date = date.ToString("dd.MM."),
                Count = reservationsLastWeek.FirstOrDefault(r => r.Date == date)?.Count ?? 0
            })
            .ToList();

        return Ok(new
        {
            RecentUsers = recentUsers,
            PopularFacilityTypes = popularTypes,
            ReservationsLastWeek = last7DaysStats
        });
    }
}