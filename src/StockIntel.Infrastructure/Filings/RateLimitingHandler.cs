namespace StockIntel.Infrastructure.Filings;

public sealed class RateLimitingHandler(EdgarRateLimiter limiter) : DelegatingHandler //HTTP middleware for the client side 
{
  protected override async Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request, CancellationToken cancellationToken)
  {
    await limiter.WaitAsync(cancellationToken); // one token per attempt
    return await base.SendAsync(request, cancellationToken);
  }
  //Primary constructor: allows you to declare a constructor directly in the class or struct header,
}