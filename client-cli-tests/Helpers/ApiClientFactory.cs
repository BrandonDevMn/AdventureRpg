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
}
