using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ApiTests;

/// <summary>
/// End-to-end flow tests. Requires the server to be running.
/// Configure the target URL in appsettings.json.
/// </summary>
public class GameFlowTests : IDisposable
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GameFlowTests()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        _client = new HttpClient { BaseAddress = new Uri(config["ServerUrl"]!) };
    }

    public void Dispose() => _client.Dispose();

    [Fact]
    public async Task FullGameFlow()
    {
        var email = $"test-{Guid.NewGuid():N}@adventure.com";
        const string password = "adventure123";

        // ── 1. Register ──────────────────────────────────────────────────────
        var registerRes = await _client.PostAsJsonAsync("/v1/auth/register", new
        {
            email,
            password
        });

        Assert.Equal(HttpStatusCode.OK, registerRes.StatusCode);

        var auth = await Deserialize<AuthResponse>(registerRes);
        Assert.False(string.IsNullOrWhiteSpace(auth.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(auth.RefreshToken));
        Assert.False(string.IsNullOrWhiteSpace(auth.UserId));

        // ── 2. Login ─────────────────────────────────────────────────────────
        var loginRes = await _client.PostAsJsonAsync("/v1/auth/login", new
        {
            email,
            password
        });

        Assert.Equal(HttpStatusCode.OK, loginRes.StatusCode);

        auth = await Deserialize<AuthResponse>(loginRes);
        Assert.False(string.IsNullOrWhiteSpace(auth.AccessToken));

        SetBearerToken(auth.AccessToken);

        // ── 3. Create character ───────────────────────────────────────────────
        var createCharRes = await _client.PostAsJsonAsync("/v1/character", new
        {
            name = "Aldric",
            @class = 0 // Warrior
        });

        Assert.Equal(HttpStatusCode.Created, createCharRes.StatusCode);

        var character = await Deserialize<Character>(createCharRes);
        Assert.Equal("Aldric", character.Name);
        Assert.Equal(0, character.Class);
        Assert.NotEqual(Guid.Empty, character.Id);

        // ── 4. List characters ────────────────────────────────────────────────
        var listRes = await _client.GetAsync("/v1/character");

        Assert.Equal(HttpStatusCode.OK, listRes.StatusCode);

        var characters = await Deserialize<List<Character>>(listRes);
        Assert.Single(characters);
        Assert.Equal(character.Id, characters[0].Id);

        // ── 5. Go fishing ─────────────────────────────────────────────────────
        var fishRes = await _client.PostAsync($"/v1/fishing/{character.Id}/cast", null);

        Assert.Equal(HttpStatusCode.OK, fishRes.StatusCode);

        var fishResult = await Deserialize<FishingResult>(fishRes);
        Assert.True(fishResult.Roll >= 1 && fishResult.Roll <= 100);
        Assert.True(fishResult.RequiredRoll >= 1);

        if (fishResult.Success)
        {
            Assert.NotNull(fishResult.CaughtItem);
            Assert.False(string.IsNullOrWhiteSpace(fishResult.CaughtItem!.Name));
            Assert.False(string.IsNullOrWhiteSpace(fishResult.CaughtItem.RarityLabel));
        }

        // ── 6. Delete character ───────────────────────────────────────────────
        var deleteCharRes = await _client.DeleteAsync($"/v1/character/{character.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteCharRes.StatusCode);

        // confirm it's gone
        var afterDeleteRes = await _client.GetAsync($"/v1/character/{character.Id}");
        Assert.Equal(HttpStatusCode.NotFound, afterDeleteRes.StatusCode);

        // ── 7. Delete account ─────────────────────────────────────────────────
        var deleteAccountRes = await _client.DeleteAsync("/v1/auth/account");

        Assert.Equal(HttpStatusCode.NoContent, deleteAccountRes.StatusCode);

        // confirm a request without a token is rejected
        _client.DefaultRequestHeaders.Authorization = null;
        var afterAccountDeleteRes = await _client.GetAsync("/v1/character");
        Assert.Equal(HttpStatusCode.Unauthorized, afterAccountDeleteRes.StatusCode);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetBearerToken(string token) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    private static async Task<T> Deserialize<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, JsonOptions)
               ?? throw new InvalidOperationException($"Failed to deserialize {typeof(T).Name}. Body: {json}");
    }

    // ── Local response models ─────────────────────────────────────────────────

    private record AuthResponse(
        string AccessToken,
        string RefreshToken,
        string UserId,
        DateTime AccessTokenExpiresAt);

    private record Character(
        Guid Id,
        string Name,
        int Class,
        int Level,
        int Strength,
        int Intelligence,
        int Agility);

    private record FishingResult(
        bool Success,
        string Message,
        int Roll,
        int RequiredRoll,
        CaughtItem? CaughtItem);

    private record CaughtItem(
        Guid Id,
        string Name,
        string Description,
        int Rarity,
        string RarityLabel);
}
