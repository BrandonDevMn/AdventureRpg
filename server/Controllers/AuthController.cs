using System.Security.Claims;
using AdventureRpg.Data;
using AdventureRpg.DTOs;
using AdventureRpg.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AdventureRpg.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/auth")]
public class AuthController(
    UserManager<IdentityUser> userManager,
    ITokenService tokenService,
    IConfiguration configuration,
    AppDbContext db) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    private async Task<AuthResponse> BuildAuthResponse(IdentityUser user)
    {
        var expiryMinutes = double.Parse(configuration["Jwt:ExpiryMinutes"]!);
        var refreshToken = await tokenService.GenerateRefreshTokenAsync(user.Id);
        return new AuthResponse
        {
            AccessToken = tokenService.GenerateAccessToken(user),
            RefreshToken = refreshToken.Token,
            UserId = user.Id,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new IdentityUser { UserName = request.Email, Email = request.Email };
        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return Ok(await BuildAuthResponse(user));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(await BuildAuthResponse(user));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var refreshToken = await tokenService.GetValidRefreshTokenAsync(request.RefreshToken);
        if (refreshToken is null)
            return Unauthorized(new { message = "Invalid or expired refresh token." });

        var user = await userManager.FindByIdAsync(refreshToken.UserId);
        if (user is null)
            return Unauthorized(new { message = "User not found." });

        await tokenService.RevokeRefreshTokenAsync(refreshToken);
        return Ok(await BuildAuthResponse(user));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
    {
        var refreshToken = await tokenService.GetValidRefreshTokenAsync(request.RefreshToken);
        if (refreshToken is null || refreshToken.UserId != UserId)
            return BadRequest(new { message = "Invalid refresh token." });

        await tokenService.RevokeRefreshTokenAsync(refreshToken);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("account")]
    public async Task<IActionResult> DeleteAccount()
    {
        var user = await userManager.FindByIdAsync(UserId);
        if (user is null)
            return NotFound();

        await tokenService.RevokeAllRefreshTokensAsync(UserId);

        var characters = db.Characters.Where(c => c.UserId == UserId).ToList();
        var characterIds = characters.Select(c => c.Id).ToList();
        var items = db.Items.Where(i => characterIds.Contains(i.CharacterId)).ToList();

        db.Items.RemoveRange(items);
        db.Characters.RemoveRange(characters);
        await db.SaveChangesAsync();

        await userManager.DeleteAsync(user);
        return NoContent();
    }
}
