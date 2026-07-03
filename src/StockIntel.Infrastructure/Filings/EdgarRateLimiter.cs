using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;

namespace StockIntel.Infrastructure.Filings;

// token bucket for EDGAR. Singleton: every EDGAR client shares this one bucket
public sealed class EdgarRateLimiter : IDisposable
{
  private readonly TokenBucketRateLimiter _limiter;

  public EdgarRateLimiter(IOptions<EdgarOptions> options)
  {
    var perSecond = options.Value.RequestPerSecond;
    _limiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
    {
      TokenLimit = perSecond, // max burst
      TokensPerPeriod = perSecond, // refill rate
      ReplenishmentPeriod = TimeSpan.FromSeconds(1),
      QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
      QueueLimit = 1000, // callers wait, FIFO
      AutoReplenishment = true
    });
  }
  public async ValueTask WaitAsync(CancellationToken cancellationToken)
  {
    using var lease = await _limiter.AcquireAsync(1, cancellationToken);
    if (!lease.IsAcquired)
      throw new InvalidOperationException("EDGAR rate limiter queue overflow. Something is issuing more requests than expected");
  }

  public void Dispose() => _limiter.Dispose();
}