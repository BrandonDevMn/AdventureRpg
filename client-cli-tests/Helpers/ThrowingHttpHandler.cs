namespace client_cli_tests.Helpers;

/// <summary>
/// An HttpMessageHandler that always throws HttpRequestException, simulating an unreachable server.
/// </summary>
public class ThrowingHttpHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken) =>
        throw new HttpRequestException("Connection refused");
}
