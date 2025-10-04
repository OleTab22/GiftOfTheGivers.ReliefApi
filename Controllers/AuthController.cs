using BCrypt.Net;
using GiftOfTheGivers.ReliefApi.Data;
using GiftOfTheGivers.ReliefApi.Models;
using GiftOfTheGivers.ReliefApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.ReliefApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ReliefDbContext db, JwtTokenService jwt) : ControllerBase
{
    public record RegisterDto(string FullName, string Email, string Password);
    public record LoginDto(string Email, string Password);

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await db.Users.AnyAsync(u => u.Email == dto.Email))
            return Conflict("Email already exists.");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "User"
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return Ok(new { user.UserId, user.FullName, user.Email });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");

        var token = jwt.Create(user);
        return Ok(new { token });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me([FromQuery] string? token)
    {
        // Fallback: allow token via query when Swagger doesn't send Authorization header
        if (!string.IsNullOrWhiteSpace(token) && !Request.Headers.ContainsKey("Authorization"))
        {
            Request.Headers.Authorization = $"Bearer {token}";
        }

        var email = User.FindFirst("email")?.Value
                    ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (string.IsNullOrWhiteSpace(email))
            return Unauthorized();

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
        if (user is null) return NotFound();

        return Ok(new { user.UserId, user.FullName, user.Email, user.Role });
    }
}


