using AdventureRpgCli.Services;

namespace client_cli_tests.Helpers;

public static class ApiClientFactory
{
    public static (ApiClient client, MockHttpHandler handler) Create()
    {
        var handler = new MockHttpHandler();
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var client = new ApiClient(http);
        return (client, handler);
    }

    /// <summary>
    /// Returns an ApiClient wired to a handler that always throws HttpRequestException,
    /// simulating a server that cannot be reached.
    /// </summary>
    public static (ApiClient client, ThrowingHttpHandler handler) CreateUnreachable()
    {
        var handler = new ThrowingHttpHandler();
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var client = new ApiClient(http);
        return (client, handler);
    }
}
