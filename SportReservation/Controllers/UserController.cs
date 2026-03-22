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
public class UserController(AppDbContext db, UserService userService) : ControllerBase
{
    /// <summary>
    /// Returns all users
    /// </summary>
    /// <returns>User entities</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (HttpContext.LoggedUser().Role != UserRole.Admin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "forbidden");
        }

        return Ok(await db.Users
            .Select(it => it.ToDto())
            .ToListAsync()
        );
    }

    /// <summary>
    /// Returns current logged user
    /// </summary>
    /// <returns>User entity</returns>
    [HttpGet("Self")]
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

    /// <summary>
    /// Updates user information
    /// </summary>
    /// <param name="patch">update data</param>
    /// <returns>Updated user</returns>
    [HttpPatch]
    public async Task<IActionResult> Update([FromBody] UserPatchDto patch)
    {
        var user = await userService.Update(HttpContext.LoggedUser(), patch);
        return Ok(user.ToDto());
    }

    /// <summary>
    /// Try to delete user
    /// </summary>
    /// <param name="id">user id</param>
    /// <returns>nothing</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await userService.Delete(HttpContext.LoggedUser(), id);
        return Ok();
    }
}