using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AdventureRpgCli.Models;

namespace AdventureRpgCli.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly SessionStore? _sessionStore;

    public ApiClient(string baseUrl, SessionStore? sessionStore = null)
        : this(new HttpClient { BaseAddress = new Uri(baseUrl) }, sessionStore) { }

    internal ApiClient(HttpClient http, SessionStore? sessionStore = null)
    {
        _http = http;
        _sessionStore = sessionStore;
    }

    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    private string? _accessToken;
    private string? _refreshToken;

    public bool IsLoggedIn => _accessToken is not null;

    // ── Health ────────────────────────────────────────────────────────────────

    public async Task<bool> CheckHealthAsync()
    {
        try
        {
            var res = await _http.GetAsync("/v1/health");
            return res.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // ── Auth ──────────────────────────────────────────────────────────────────

    public async Task<AuthResponse> RegisterAsync(string email, string password)
    {
        var res = await _http.PostAsJsonAsync("/v1/auth/register", new { email, password });
        await EnsureSuccess(res);
        var auth = await Deserialize<AuthResponse>(res);
        StoreTokens(auth);
        return auth;
    }

    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        var res = await _http.PostAsJsonAsync("/v1/auth/login", new { email, password });
        await EnsureSuccess(res);
        var auth = await Deserialize<AuthResponse>(res);
        StoreTokens(auth);
        return auth;
    }

    public async Task LogoutAsync()
    {
        if (_refreshToken is null) return;
        var res = await SendAuthorized(HttpMethod.Post, "/v1/auth/logout",
            new { refreshToken = _refreshToken });
        await EnsureSuccess(res);
        ClearTokens();
    }

    public async Task DeleteAccountAsync()
    {
        var res = await SendAuthorized(HttpMethod.Delete, "/v1/auth/account");
        await EnsureSuccess(res);
        ClearTokens();
    }

    /// <summary>
    /// Attempts to restore a previous session using a persisted refresh token.
    /// Returns true if the session was successfully restored.
    /// </summary>
    public async Task<bool> RestoreSessionAsync(string refreshToken)
    {
        _refreshToken = refreshToken;
        var success = await TryRefreshAsync();
        if (!success)
            _refreshToken = null;
        return success;
    }

    // ── Characters ────────────────────────────────────────────────────────────

    public async Task<List<Character>> GetCharactersAsync()
    {
        var res = await SendAuthorized(HttpMethod.Get, "/v1/character");
        await EnsureSuccess(res);
        return await Deserialize<List<Character>>(res);
    }

    public async Task<Character> CreateCharacterAsync(string name, int characterClass)
    {
        var res = await SendAuthorized(HttpMethod.Post, "/v1/character",
            new { name, @class = characterClass });
        await EnsureSuccess(res);
        return await Deserialize<Character>(res);
    }

    public async Task DeleteCharacterAsync(Guid id)
    {
        var res = await SendAuthorized(HttpMethod.Delete, $"/v1/character/{id}");
        await EnsureSuccess(res);
    }

    // ── Fishing ───────────────────────────────────────────────────────────────

    public async Task<FishingResult> CastAsync(Guid characterId)
    {
        var res = await SendAuthorized(HttpMethod.Post, $"/v1/fishing/{characterId}/cast");
        await EnsureSuccess(res);
        return await Deserialize<FishingResult>(res);
    }

    // ── Inventory ─────────────────────────────────────────────────────────────

    public async Task<List<Item>> GetInventoryAsync(Guid characterId)
    {
        var res = await SendAuthorized(HttpMethod.Get, $"/v1/inventory/{characterId}");
        await EnsureSuccess(res);
        var inv = await Deserialize<InventoryResponse>(res);
        return inv.Items;
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private async Task<HttpResponseMessage> SendAuthorized(
        HttpMethod method, string path, object? body = null)
    {
        var res = await Send(method, path, body);

        if (res.StatusCode == HttpStatusCode.Unauthorized && _refreshToken is not null)
        {
            var refreshed = await TryRefreshAsync();
            if (refreshed)
                res = await Send(method, path, body);
        }

        return res;
    }

    private async Task<HttpResponseMessage> Send(HttpMethod method, string path, object? body = null)
    {
        var req = new HttpRequestMessage(method, path);
        if (_accessToken is not null)
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        if (body is not null)
            req.Content = JsonContent.Create(body);
        return await _http.SendAsync(req);
    }

    private async Task<bool> TryRefreshAsync()
    {
        try
        {
            var res = await _http.PostAsJsonAsync("/v1/auth/refresh",
                new { refreshToken = _refreshToken });
            if (!res.IsSuccessStatusCode) return false;
            var auth = await Deserialize<AuthResponse>(res);
            StoreTokens(auth);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void StoreTokens(AuthResponse auth)
    {
        _accessToken = auth.AccessToken;
        _refreshToken = auth.RefreshToken;
        _sessionStore?.Save(auth.RefreshToken);
    }

    private void ClearTokens()
    {
        _accessToken = null;
        _refreshToken = null;
        _sessionStore?.Clear();
    }

    private static async Task<T> Deserialize<T>(HttpResponseMessage res)
    {
        var json = await res.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, Json)
               ?? throw new InvalidOperationException($"Could not deserialize response: {json}");
    }

    private static async Task EnsureSuccess(HttpResponseMessage res)
    {
        if (res.IsSuccessStatusCode) return;
        var body = await res.Content.ReadAsStringAsync();
        throw new ApiException((int)res.StatusCode, body);
    }
}

public class ApiException(int statusCode, string body) : Exception($"API error {statusCode}: {body}")
{
    public int StatusCode { get; } = statusCode;
    public string Body { get; } = body;
}
