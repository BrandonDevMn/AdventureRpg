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

    [Fact]
    public void GenerateToken_ReturnsNonEmptyJwtString()
    {
        var service = new TokenService(BuildConfig());

        var token = service.GenerateToken(TestUser());

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void GenerateToken_ContainsUserIdClaim()
    {
        var user = TestUser();
        var service = new TokenService(BuildConfig());

        var token = service.GenerateToken(user);
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Contains(parsed.Claims, c => c.Value == user.Id);
    }

    [Fact]
    public void GenerateToken_ContainsEmailClaim()
    {
        var user = TestUser();
        var service = new TokenService(BuildConfig());

        var token = service.GenerateToken(user);
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Contains(parsed.Claims, c => c.Value == user.Email);
    }

    [Fact]
    public void GenerateToken_TokenExpiresAfterConfiguredMinutes()
    {
        var service = new TokenService(BuildConfig());

        var before = DateTime.UtcNow.AddMinutes(59);
        var token = service.GenerateToken(TestUser());
        var after = DateTime.UtcNow.AddMinutes(61);

        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.True(parsed.ValidTo > before && parsed.ValidTo < after);
    }
}
