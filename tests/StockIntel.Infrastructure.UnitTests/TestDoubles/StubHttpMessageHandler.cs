using System.Net;
using System.Text;

namespace StockIntel.Infrastructure.UnitTests.TestDoubles;

public sealed class StubHttpMessageHandler : HttpMessageHandler
{
  private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
  public List<HttpRequestMessage> Requests { get; } = new();

  public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    => _responder = responder;

  public static StubHttpMessageHandler RespondingWithJson(string json) => 
    new (_ => new HttpResponseMessage(HttpStatusCode.OK)
    {
      Content = new StringContent(json, Encoding.UTF8, "application/json")
    });

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
  {
    Requests.Add(request);
    return Task.FromResult(_responder(request));
  }
}