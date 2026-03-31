using AdventureRpg.DTOs;
using AdventureRpg.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AdventureRpg.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/auth")]
public class AuthController(UserManager<IdentityUser> userManager, ITokenService tokenService, IConfiguration configuration) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new IdentityUser { UserName = request.Email, Email = request.Email };
        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        var expiryMinutes = double.Parse(configuration["Jwt:ExpiryMinutes"]!);
        return Ok(new AuthResponse
        {
            Token = tokenService.GenerateToken(user),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new { message = "Invalid email or password." });

        var expiryMinutes = double.Parse(configuration["Jwt:ExpiryMinutes"]!);
        return Ok(new AuthResponse
        {
            Token = tokenService.GenerateToken(user),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
        });
    }
}
