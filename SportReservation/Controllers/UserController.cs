using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportReservation.Data;
using SportReservation.Middlewares;
using SportReservation.Models;
using SportReservation.Services;

namespace SportReservation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(AppDbContext db, UserService userService) : ControllerBase
{
    /// <summary>
    /// Returns current logged user
    /// </summary>
    /// <returns>User entity</returns>
    [HttpGet]
    public IActionResult Self()
    {
        return Ok(HttpContext.LoggedUser().ToDto());
    }
    
    /// <summary>
    /// Registers new user
    /// </summary>
    /// <param name="body">dto</param>
    /// <returns>created user</returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto body)
    {
        var user = await userService.Register(body, UserRole.User);
        return Ok(user.ToDto());
    }

    [HttpPatch]
    public async Task<IActionResult> Update([FromBody] UserPatchDto patch)
    {
        var user = await userService.Update(HttpContext.LoggedUser(), patch);
        return Ok(user.ToDto());
    }
}