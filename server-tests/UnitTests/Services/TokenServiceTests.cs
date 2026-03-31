using System.IdentityModel.Tokens.Jwt;
using AdventureRpg.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace UnitTests.Services;

public class TokenServiceTests
{
    private static IConfiguration BuildConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"]           = "TestSecretKey_MustBe32CharsOrMore!!",
                ["Jwt:Issuer"]        = "TestIssuer",
                ["Jwt:Audience"]      = "TestAudience",
                ["Jwt:ExpiryMinutes"] = "60"
            })
            .Build();

    private static IdentityUser TestUser() => new()
    {
        Id = Guid.NewGuid().ToString(),
        Email = "hero@adventure.com",
        UserName = "hero@adventure.com"
    };

    private static AdventureRpg.Services.TokenService CreateService(string dbName) =>
        new(BuildConfig(), UnitTests.Helpers.DbContextFactory.Create(dbName));

    [Fact]
    public void GenerateAccessToken_ReturnsNonEmptyJwtString()
    {
        var token = CreateService("ts_access").GenerateAccessToken(TestUser());
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void GenerateAccessToken_ContainsUserIdClaim()
    {
        var user = TestUser();
        var token = CreateService("ts_userid").GenerateAccessToken(user);
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Contains(parsed.Claims, c => c.Value == user.Id);
    }

    [Fact]
    public void GenerateAccessToken_ContainsEmailClaim()
    {
        var user = TestUser();
        var token = CreateService("ts_email").GenerateAccessToken(user);
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Contains(parsed.Claims, c => c.Value == user.Email);
    }

    [Fact]
    public void GenerateAccessToken_TokenExpiresAfterConfiguredMinutes()
    {
        var before = DateTime.UtcNow.AddMinutes(59);
        var token = CreateService("ts_expiry").GenerateAccessToken(TestUser());
        var after = DateTime.UtcNow.AddMinutes(61);
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.True(parsed.ValidTo > before && parsed.ValidTo < after);
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_PersistsTokenToDb()
    {
        var service = CreateService("ts_refresh_persist");
        var userId = Guid.NewGuid().ToString();

        var refreshToken = await service.GenerateRefreshTokenAsync(userId);

        Assert.False(string.IsNullOrWhiteSpace(refreshToken.Token));
        Assert.Equal(userId, refreshToken.UserId);
        Assert.False(refreshToken.IsRevoked);
    }

    [Fact]
    public async Task GetValidRefreshTokenAsync_ValidToken_ReturnsToken()
    {
        var service = CreateService("ts_get_valid");
        var created = await service.GenerateRefreshTokenAsync("user-1");

        var result = await service.GetValidRefreshTokenAsync(created.Token);

        Assert.NotNull(result);
        Assert.Equal(created.Token, result.Token);
    }

    [Fact]
    public async Task GetValidRefreshTokenAsync_RevokedToken_ReturnsNull()
    {
        var service = CreateService("ts_get_revoked");
        var created = await service.GenerateRefreshTokenAsync("user-1");
        await service.RevokeRefreshTokenAsync(created);

        var result = await service.GetValidRefreshTokenAsync(created.Token);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetValidRefreshTokenAsync_NonExistentToken_ReturnsNull()
    {
        var service = CreateService("ts_get_nonexistent");

        var result = await service.GetValidRefreshTokenAsync("does-not-exist");

        Assert.Null(result);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_SetsIsRevokedTrue()
    {
        var service = CreateService("ts_revoke");
        var created = await service.GenerateRefreshTokenAsync("user-1");

        await service.RevokeRefreshTokenAsync(created);

        Assert.True(created.IsRevoked);
    }

    [Fact]
    public async Task RevokeAllRefreshTokensAsync_RevokesOnlyUsersActiveTokens()
    {
        var service = CreateService("ts_revoke_all");
        var t1 = await service.GenerateRefreshTokenAsync("user-1");
        var t2 = await service.GenerateRefreshTokenAsync("user-1");
        var other = await service.GenerateRefreshTokenAsync("user-2");

        await service.RevokeAllRefreshTokensAsync("user-1");

        Assert.True(t1.IsRevoked);
        Assert.True(t2.IsRevoked);
        Assert.False(other.IsRevoked);
    }
}
