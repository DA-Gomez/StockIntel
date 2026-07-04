namespace StockIntel.Infrastructure.UnitTests.TestDoubles;

public sealed class FakeHttpClientFactory(HttpClient client) : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => client;
}