using AdventureRpg.Controllers;
using AdventureRpg.DTOs;
using AdventureRpg.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace UnitTests.Controllers;

public class AuthControllerTests
{
    private static IConfiguration BuildConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:ExpiryMinutes"] = "60"
            })
            .Build();

    private static Mock<UserManager<IdentityUser>> BuildUserManagerMock()
    {
        var store = new Mock<IUserStore<IdentityUser>>();
        return new Mock<UserManager<IdentityUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private AuthController CreateController(
        Mock<UserManager<IdentityUser>> userManagerMock,
        Mock<ITokenService> tokenMock) =>
        new(userManagerMock.Object, tokenMock.Object, BuildConfig());

    // ── Register ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_Success_ReturnsOkWithToken()
    {
        var userManagerMock = BuildUserManagerMock();
        var tokenMock = new Mock<ITokenService>();

        userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        tokenMock
            .Setup(t => t.GenerateToken(It.IsAny<IdentityUser>()))
            .Returns("signed.jwt.token");

        var result = await CreateController(userManagerMock, tokenMock)
            .Register(new RegisterRequest { Email = "hero@rpg.com", Password = "pass123" });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(ok.Value);
        Assert.Equal("signed.jwt.token", response.Token);
    }

    [Fact]
    public async Task Register_IdentityFailure_ReturnsBadRequest()
    {
        var userManagerMock = BuildUserManagerMock();
        var tokenMock = new Mock<ITokenService>();

        userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email already taken." }));

        var result = await CreateController(userManagerMock, tokenMock)
            .Register(new RegisterRequest { Email = "hero@rpg.com", Password = "pass123" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        var userManagerMock = BuildUserManagerMock();
        var tokenMock = new Mock<ITokenService>();
        var user = new IdentityUser { Id = "u1", Email = "hero@rpg.com" };

        userManagerMock.Setup(m => m.FindByEmailAsync("hero@rpg.com")).ReturnsAsync(user);
        userManagerMock.Setup(m => m.CheckPasswordAsync(user, "pass123")).ReturnsAsync(true);
        tokenMock.Setup(t => t.GenerateToken(user)).Returns("signed.jwt.token");

        var result = await CreateController(userManagerMock, tokenMock)
            .Login(new LoginRequest { Email = "hero@rpg.com", Password = "pass123" });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(ok.Value);
        Assert.Equal("signed.jwt.token", response.Token);
    }

    [Fact]
    public async Task Login_UserNotFound_ReturnsUnauthorized()
    {
        var userManagerMock = BuildUserManagerMock();
        var tokenMock = new Mock<ITokenService>();

        userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((IdentityUser?)null);

        var result = await CreateController(userManagerMock, tokenMock)
            .Login(new LoginRequest { Email = "ghost@rpg.com", Password = "pass123" });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var userManagerMock = BuildUserManagerMock();
        var tokenMock = new Mock<ITokenService>();
        var user = new IdentityUser { Id = "u1", Email = "hero@rpg.com" };

        userManagerMock.Setup(m => m.FindByEmailAsync("hero@rpg.com")).ReturnsAsync(user);
        userManagerMock.Setup(m => m.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(false);

        var result = await CreateController(userManagerMock, tokenMock)
            .Login(new LoginRequest { Email = "hero@rpg.com", Password = "wrongpass" });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}
