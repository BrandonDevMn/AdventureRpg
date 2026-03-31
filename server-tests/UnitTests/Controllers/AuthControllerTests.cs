using AdventureRpg.Controllers;
using AdventureRpg.Data;
using AdventureRpg.DTOs;
using AdventureRpg.Models;
using AdventureRpg.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using UnitTests.Helpers;
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

    private static IdentityUser TestUser() => new() { Id = "u1", Email = "hero@rpg.com", UserName = "hero@rpg.com" };

    private AuthController CreateController(
        Mock<UserManager<IdentityUser>> userManagerMock,
        Mock<ITokenService> tokenMock,
        AppDbContext? db = null,
        string? userId = null)
    {
        var controller = new AuthController(
            userManagerMock.Object,
            tokenMock.Object,
            BuildConfig(),
            db ?? DbContextFactory.Create($"auth_{Guid.NewGuid()}"));

        if (userId is not null)
            controller.SetUser(userId);

        return controller;
    }

    private static Mock<ITokenService> BuildTokenMock(string refreshTokenValue = "refresh-token-abc")
    {
        var mock = new Mock<ITokenService>();
        mock.Setup(t => t.GenerateAccessToken(It.IsAny<IdentityUser>())).Returns("access.jwt.token");
        mock.Setup(t => t.GenerateRefreshTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(new RefreshToken { Token = refreshTokenValue });
        return mock;
    }

    // ── Register ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_Success_ReturnsOkWithTokens()
    {
        var userManagerMock = BuildUserManagerMock();
        userManagerMock.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await CreateController(userManagerMock, BuildTokenMock())
            .Register(new RegisterRequest { Email = "hero@rpg.com", Password = "pass123" });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(ok.Value);
        Assert.Equal("access.jwt.token", response.AccessToken);
        Assert.Equal("refresh-token-abc", response.RefreshToken);
    }

    [Fact]
    public async Task Register_IdentityFailure_ReturnsBadRequest()
    {
        var userManagerMock = BuildUserManagerMock();
        userManagerMock.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email taken." }));

        var result = await CreateController(userManagerMock, BuildTokenMock())
            .Register(new RegisterRequest { Email = "hero@rpg.com", Password = "pass123" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithTokens()
    {
        var user = TestUser();
        var userManagerMock = BuildUserManagerMock();
        userManagerMock.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        userManagerMock.Setup(m => m.CheckPasswordAsync(user, "pass123")).ReturnsAsync(true);

        var result = await CreateController(userManagerMock, BuildTokenMock())
            .Login(new LoginRequest { Email = user.Email!, Password = "pass123" });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(ok.Value);
        Assert.Equal("access.jwt.token", response.AccessToken);
    }

    [Fact]
    public async Task Login_UserNotFound_ReturnsUnauthorized()
    {
        var userManagerMock = BuildUserManagerMock();
        userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((IdentityUser?)null);

        var result = await CreateController(userManagerMock, BuildTokenMock())
            .Login(new LoginRequest { Email = "ghost@rpg.com", Password = "pass123" });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var user = TestUser();
        var userManagerMock = BuildUserManagerMock();
        userManagerMock.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        userManagerMock.Setup(m => m.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(false);

        var result = await CreateController(userManagerMock, BuildTokenMock())
            .Login(new LoginRequest { Email = user.Email!, Password = "wrong" });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    // ── Refresh ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Refresh_ValidToken_ReturnsNewTokens()
    {
        var user = TestUser();
        var storedToken = new RefreshToken { Token = "old-refresh", UserId = user.Id };
        var tokenMock = BuildTokenMock("new-refresh-token");
        tokenMock.Setup(t => t.GetValidRefreshTokenAsync("old-refresh")).ReturnsAsync(storedToken);
        tokenMock.Setup(t => t.RevokeRefreshTokenAsync(storedToken)).Returns(Task.CompletedTask);

        var userManagerMock = BuildUserManagerMock();
        userManagerMock.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await CreateController(userManagerMock, tokenMock)
            .Refresh(new RefreshRequest { RefreshToken = "old-refresh" });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(ok.Value);
        Assert.Equal("new-refresh-token", response.RefreshToken);
    }

    [Fact]
    public async Task Refresh_InvalidToken_ReturnsUnauthorized()
    {
        var tokenMock = new Mock<ITokenService>();
        tokenMock.Setup(t => t.GetValidRefreshTokenAsync(It.IsAny<string>())).ReturnsAsync((RefreshToken?)null);

        var result = await CreateController(BuildUserManagerMock(), tokenMock)
            .Refresh(new RefreshRequest { RefreshToken = "bad-token" });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Refresh_UserNotFound_ReturnsUnauthorized()
    {
        var storedToken = new RefreshToken { Token = "rt", UserId = "deleted-user" };
        var tokenMock = new Mock<ITokenService>();
        tokenMock.Setup(t => t.GetValidRefreshTokenAsync("rt")).ReturnsAsync(storedToken);

        var userManagerMock = BuildUserManagerMock();
        userManagerMock.Setup(m => m.FindByIdAsync("deleted-user")).ReturnsAsync((IdentityUser?)null);

        var result = await CreateController(userManagerMock, tokenMock)
            .Refresh(new RefreshRequest { RefreshToken = "rt" });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Logout_ValidToken_RevokesAndReturnsNoContent()
    {
        var user = TestUser();
        var storedToken = new RefreshToken { Token = "rt", UserId = user.Id };
        var tokenMock = new Mock<ITokenService>();
        tokenMock.Setup(t => t.GetValidRefreshTokenAsync("rt")).ReturnsAsync(storedToken);
        tokenMock.Setup(t => t.RevokeRefreshTokenAsync(storedToken)).Returns(Task.CompletedTask);

        var result = await CreateController(BuildUserManagerMock(), tokenMock, userId: user.Id)
            .Logout(new RefreshRequest { RefreshToken = "rt" });

        Assert.IsType<NoContentResult>(result);
        tokenMock.Verify(t => t.RevokeRefreshTokenAsync(storedToken), Times.Once);
    }

    [Fact]
    public async Task Logout_InvalidToken_ReturnsBadRequest()
    {
        var tokenMock = new Mock<ITokenService>();
        tokenMock.Setup(t => t.GetValidRefreshTokenAsync(It.IsAny<string>())).ReturnsAsync((RefreshToken?)null);

        var result = await CreateController(BuildUserManagerMock(), tokenMock, userId: "u1")
            .Logout(new RefreshRequest { RefreshToken = "bad" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Logout_TokenBelongsToOtherUser_ReturnsBadRequest()
    {
        var storedToken = new RefreshToken { Token = "rt", UserId = "other-user" };
        var tokenMock = new Mock<ITokenService>();
        tokenMock.Setup(t => t.GetValidRefreshTokenAsync("rt")).ReturnsAsync(storedToken);

        var result = await CreateController(BuildUserManagerMock(), tokenMock, userId: "u1")
            .Logout(new RefreshRequest { RefreshToken = "rt" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ── Delete Account ────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAccount_ValidUser_DeletesEverythingAndReturnsNoContent()
    {
        var user = TestUser();
        var db = DbContextFactory.Create($"delete_account_{Guid.NewGuid()}");
        var tokenMock = new Mock<ITokenService>();
        tokenMock.Setup(t => t.RevokeAllRefreshTokensAsync(user.Id)).Returns(Task.CompletedTask);

        var userManagerMock = BuildUserManagerMock();
        userManagerMock.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        userManagerMock.Setup(m => m.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await CreateController(userManagerMock, tokenMock, db, userId: user.Id)
            .DeleteAccount();

        Assert.IsType<NoContentResult>(result);
        tokenMock.Verify(t => t.RevokeAllRefreshTokensAsync(user.Id), Times.Once);
        userManagerMock.Verify(m => m.DeleteAsync(user), Times.Once);
    }

    [Fact]
    public async Task DeleteAccount_UserNotFound_ReturnsNotFound()
    {
        var userManagerMock = BuildUserManagerMock();
        userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((IdentityUser?)null);

        var result = await CreateController(userManagerMock, BuildTokenMock(), userId: "u1")
            .DeleteAccount();

        Assert.IsType<NotFoundResult>(result);
    }
}
