using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AdventureRpg.Data;
using AdventureRpg.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace AdventureRpg.Services;

public interface ITokenService
{
    string GenerateAccessToken(IdentityUser user);
    Task<RefreshToken> GenerateRefreshTokenAsync(string userId);
    Task<RefreshToken?> GetValidRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(RefreshToken refreshToken);
    Task RevokeAllRefreshTokensAsync(string userId);
}

public class TokenService(IConfiguration configuration, AppDbContext db) : ITokenService
{
    public string GenerateAccessToken(IdentityUser user)
    {
        var jwtConfig = configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(double.Parse(jwtConfig["ExpiryMinutes"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
        };

        var token = new JwtSecurityToken(
            issuer: jwtConfig["Issuer"],
            audience: jwtConfig["Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(string userId)
    {
        var token = new RefreshToken
        {
            UserId = userId,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
        };
        db.RefreshTokens.Add(token);
        await db.SaveChangesAsync();
        return token;
    }

    public Task<RefreshToken?> GetValidRefreshTokenAsync(string token) =>
        Task.FromResult(db.RefreshTokens
            .FirstOrDefault(r => r.Token == token && !r.IsRevoked));

    public async Task RevokeRefreshTokenAsync(RefreshToken refreshToken)
    {
        refreshToken.IsRevoked = true;
        await db.SaveChangesAsync();
    }

    public async Task RevokeAllRefreshTokensAsync(string userId)
    {
        var tokens = db.RefreshTokens.Where(r => r.UserId == userId && !r.IsRevoked).ToList();
        foreach (var t in tokens)
            t.IsRevoked = true;
        await db.SaveChangesAsync();
    }
}
