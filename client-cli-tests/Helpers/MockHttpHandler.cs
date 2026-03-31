using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace client_cli_tests.Helpers;

/// <summary>
/// Queues predetermined HTTP responses so ApiClient can be tested without a real server.
/// </summary>
public class MockHttpHandler : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _queue = new();
    public List<HttpRequestMessage> Requests { get; } = [];

    public void Enqueue(HttpStatusCode status, object? body = null)
    {
        var response = new HttpResponseMessage(status);
        if (body is not null)
            response.Content = JsonContent.Create(body,
                options: new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        _queue.Enqueue(response);
    }

    public void EnqueueEmpty(HttpStatusCode status) =>
        _queue.Enqueue(new HttpResponseMessage(status));

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        return Task.FromResult(_queue.Count > 0
            ? _queue.Dequeue()
            : new HttpResponseMessage(HttpStatusCode.InternalServerError));
    }
}
