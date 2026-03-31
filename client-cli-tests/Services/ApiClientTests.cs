using System.Net;
using AdventureRpgCli.Services;
using client_cli_tests.Helpers;
using Xunit;

namespace client_cli_tests.Services;

public class ApiClientTests
{
    // ── Auth state ────────────────────────────────────────────────────────────

    [Fact]
    public void IsLoggedIn_InitiallyFalse()
    {
        var (client, _) = ApiClientFactory.Create();
        Assert.False(client.IsLoggedIn);
    }

    [Fact]
    public async Task IsLoggedIn_TrueAfterLogin()
    {
        var (client, handler) = ApiClientFactory.Create();
        handler.Enqueue(HttpStatusCode.OK, AuthPayload());

        await client.LoginAsync("a@b.com", "pass");

        Assert.True(client.IsLoggedIn);
    }

    [Fact]
    public async Task IsLoggedIn_FalseAfterLogout()
    {
        var (client, handler) = ApiClientFactory.Create();
        handler.Enqueue(HttpStatusCode.OK, AuthPayload());
        await client.LoginAsync("a@b.com", "pass");

        handler.EnqueueEmpty(HttpStatusCode.NoContent);
        await client.LogoutAsync();

        Assert.False(client.IsLoggedIn);
    }

    // ── Register ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_Success_ReturnsAuthResponse()
    {
        var (client, handler) = ApiClientFactory.Create();
        handler.Enqueue(HttpStatusCode.OK, AuthPayload());

        var result = await client.RegisterAsync("a@b.com", "pass");

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
    }

    [Fact]
    public async Task RegisterAsync_Failure_ThrowsApiException()
    {
        var (client, handler) = ApiClientFactory.Create();
        handler.EnqueueEmpty(HttpStatusCode.BadRequest);

        await Assert.ThrowsAsync<ApiException>(() => client.RegisterAsync("a@b.com", "bad"));
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_Success_StoresTokens()
    {
        var (client, handler) = ApiClientFactory.Create();
        handler.Enqueue(HttpStatusCode.OK, AuthPayload());

        await client.LoginAsync("a@b.com", "pass");

        Assert.True(client.IsLoggedIn);
    }

    [Fact]
    public async Task LoginAsync_Failure_ThrowsApiException()
    {
        var (client, handler) = ApiClientFactory.Create();
        handler.EnqueueEmpty(HttpStatusCode.Unauthorized);

        await Assert.ThrowsAsync<ApiException>(() => client.LoginAsync("a@b.com", "wrong"));
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LogoutAsync_WhenNotLoggedIn_DoesNotCallApi()
    {
        var (client, handler) = ApiClientFactory.Create();

        await client.LogoutAsync(); // should be a no-op

        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task LogoutAsync_WhenLoggedIn_CallsApiAndClearsTokens()
    {
        var (client, handler) = ApiClientFactory.Create();
        handler.Enqueue(HttpStatusCode.OK, AuthPayload());
        await client.LoginAsync("a@b.com", "pass");

        handler.EnqueueEmpty(HttpStatusCode.NoContent);
        await client.LogoutAsync();

        Assert.False(client.IsLoggedIn);
    }

    // ── Delete account ────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAccountAsync_Success_ClearsTokens()
    {
        var (client, handler) = ApiClientFactory.Create();
        handler.Enqueue(HttpStatusCode.OK, AuthPayload());
        await client.LoginAsync("a@b.com", "pass");

        handler.EnqueueEmpty(HttpStatusCode.NoContent);
        await client.DeleteAccountAsync();

        Assert.False(client.IsLoggedIn);
    }

    [Fact]
    public async Task DeleteAccountAsync_Failure_ThrowsApiException()
    {
        var (client, handler) = ApiClientFactory.Create();
        handler.Enqueue(HttpStatusCode.OK, AuthPayload());
        await client.LoginAsync("a@b.com", "pass");

        handler.EnqueueEmpty(HttpStatusCode.InternalServerError);

        await Assert.ThrowsAsync<ApiException>(() => client.DeleteAccountAsync());
    }

    // ── Characters ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCharactersAsync_Success_ReturnsList()
    {
        var (client, handler) = ApiClientFactory.Create();
        handler.Enqueue(HttpStatusCode.OK, AuthPayload());
        await client.LoginAsync("a@b.com", "pass");

        handler.Enqueue(HttpStatusCode.OK, new[] { CharacterPayload() });
        var result = await client.GetCharactersAsync();

        Assert.Single(result);
        Assert.Equal("Aldric", result[0].Name);
    }

    [Fact]
    public async Task CreateCharacterAsync_Success_ReturnsCharacter()
    {
        var (client, handler) = ApiClientFactory.Create();
        handler.Enqueue(HttpStatusCode.OK, AuthPayload());
        await client.LoginAsync("a@b.com", "pass");

        handler.Enqueue(HttpStatusCode.Created, CharacterPayload());
        var result = await client.CreateCharacterAsync("Aldric", 0);

        Assert.Equal("Aldric", result.Name);
    }

    [Fact]
    public async Task DeleteCharacterAsync_Success_DoesNotThrow()
    {
        var (client, handler) = ApiClientFactory.Create();
        handler.Enqueue(HttpStatusCode.OK, AuthPayload());
        await client.LoginAsync("a@b.com", "pass");

        handler.EnqueueEmpty(HttpStatusCode.NoContent);

        var exception = await Record.ExceptionAsync(() => client.DeleteCharacterAsync(Guid.NewGuid()));
        Assert.Null(exception);
    }

    [Fact]
    public async Task DeleteCharacterAsync_Failure_ThrowsApiException()
    {
        var (client, handler) = ApiClientFactory.Create();
        handler.Enqueue(HttpStatusCode.OK, AuthPayload());
        await client.LoginAsync("a@b.com", "pass");

        handler.EnqueueEmpty(HttpStatusCode.NotFound);

        await Assert.ThrowsAsync<ApiException>(() => client.DeleteCharacterAsync(Guid.NewGuid()));
    }

    // ── Fishing ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task CastAsync_Success_ReturnsFishingResult()
    {
        var (client, handler) = ApiClientFactory.Create();
        handler.Enqueue(HttpStatusCode.OK, AuthPayload());
        await client.LoginAsync("a@b.com", "pass");

        handler.Enqueue(HttpStatusCode.OK, new
        {
            success = true,
            message = "You caught a Minnow!",
            roll = 10,
            requiredRoll = 50,
            caughtItem = new { id = Guid.NewGuid(), name = "Minnow", description = "A tiny fish.", rarity = 1, rarityLabel = "Common" }
        });

        var result = await client.CastAsync(Guid.NewGuid());

        Assert.True(result.Success);
        Assert.Equal("Minnow", result.CaughtItem!.Name);
    }

    // ── Inventory ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetInventoryAsync_Success_ReturnsItems()
    {
        var (client, handler) = ApiClientFactory.Create();
        handler.Enqueue(HttpStatusCode.OK, AuthPayload());
        await client.LoginAsync("a@b.com", "pass");

        var charId = Guid.NewGuid();
        handler.Enqueue(HttpStatusCode.OK, new
        {
            characterId = charId,
            items = new[]
            {
                new { id = Guid.NewGuid(), name = "Perch", type = "Fish", description = "A common fish.", rarity = 1, acquiredAt = DateTime.UtcNow }
            }
        });

        var items = await client.GetInventoryAsync(charId);

        Assert.Single(items);
        Assert.Equal("Perch", items[0].Name);
    }

    // ── Auto-refresh on 401 ───────────────────────────────────────────────────

    [Fact]
    public async Task SendAuthorized_On401_RefreshesAndRetries()
    {
        var (client, handler) = ApiClientFactory.Create();
        handler.Enqueue(HttpStatusCode.OK, AuthPayload());
        await client.LoginAsync("a@b.com", "pass");

        // First attempt → 401
        handler.EnqueueEmpty(HttpStatusCode.Unauthorized);
        // Refresh call → success
        handler.Enqueue(HttpStatusCode.OK, AuthPayload("new-access", "new-refresh"));
        // Retry → success
        handler.Enqueue(HttpStatusCode.OK, new[] { CharacterPayload() });

        var result = await client.GetCharactersAsync();

        Assert.Single(result);
    }

    [Fact]
    public async Task SendAuthorized_On401_RefreshFails_ThrowsApiException()
    {
        var (client, handler) = ApiClientFactory.Create();
        handler.Enqueue(HttpStatusCode.OK, AuthPayload());
        await client.LoginAsync("a@b.com", "pass");

        // First attempt → 401
        handler.EnqueueEmpty(HttpStatusCode.Unauthorized);
        // Refresh call → fails
        handler.EnqueueEmpty(HttpStatusCode.Unauthorized);
        // Retry → still 401
        handler.EnqueueEmpty(HttpStatusCode.Unauthorized);

        await Assert.ThrowsAsync<ApiException>(() => client.GetCharactersAsync());
    }

    [Fact]
    public async Task SendAuthorized_On401_NoRefreshToken_ThrowsApiException()
    {
        var (client, handler) = ApiClientFactory.Create();

        // Not logged in — no tokens — but still call a protected endpoint
        handler.EnqueueEmpty(HttpStatusCode.Unauthorized);

        await Assert.ThrowsAsync<ApiException>(() => client.GetCharactersAsync());
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static object AuthPayload(string access = "access-token", string refresh = "refresh-token") => new
    {
        accessToken = access,
        refreshToken = refresh,
        userId = "user-1",
        accessTokenExpiresAt = DateTime.UtcNow.AddHours(1)
    };

    private static object CharacterPayload() => new
    {
        id = Guid.NewGuid(),
        name = "Aldric",
        @class = 0,
        level = 1,
        strength = 10,
        intelligence = 4,
        agility = 6
    };
}
