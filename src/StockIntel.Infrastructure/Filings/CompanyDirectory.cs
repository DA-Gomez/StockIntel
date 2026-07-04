using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StockIntel.Application.Abstractions.Filings;
using StockIntel.Domain.Common;
using StockIntel.Domain.Filings;

namespace StockIntel.Infrastructure.Filings;

//Ticker → CIK resolution backed by EDGAR's company_tickers.json (~10k rows).
// Singleton: the parsed index is cached in-process for 24h. Reference data with a slow change rate -> long in-memory cache
public sealed class CompanyDirectory : ICompanyDirectory
{
  private static readonly TimeSpan CacheLifetime = TimeSpan.FromHours(24);

  private readonly IHttpClientFactory _httpClientFactory; // NOT HttpClient
  private readonly ILogger<CompanyDirectory> _logger;
  private readonly SemaphoreSlim _refreshLock = new(1, 1);

  private Dictionary<string, CompanyIdentity>? _byTicker;
  private DateTime _expiresAtUtc = DateTime.MinValue;

  public CompanyDirectory(IHttpClientFactory httpClientFactory, ILogger<CompanyDirectory> logger)
  {
    _httpClientFactory = httpClientFactory;
    _logger = logger;
  }

  public async Task<CompanyIdentity?> ResolveAsync(Ticker ticker, CancellationToken cancellationToken)
  {
      var index = await GetIndexAsync(cancellationToken);
      return index.TryGetValue(ticker.Symbol, out var identity) ? identity : null;
  }

  private async Task<Dictionary<string, CompanyIdentity>> GetIndexAsync(CancellationToken cancellationToken)
  {
    if (_byTicker is not null && DateTime.UtcNow < _expiresAtUtc) return _byTicker;

    await _refreshLock.WaitAsync(cancellationToken);
    try
    {
      if (_byTicker is not null && DateTime.UtcNow < _expiresAtUtc) return _byTicker;

      var client = _httpClientFactory.CreateClient(EdgarHttp.DirectoryClientName);
      var entries = await client.GetFromJsonAsync<Dictionary<string, CompanyTickerEntry>>(EdgarHttp.CompanyTickersUrl, cancellationToken)
        ?? throw new InvalidOperationException("company_tickers.json deserialized to null");

      var index = new Dictionary<string, CompanyIdentity>(StringComparer.OrdinalIgnoreCase);
      foreach (var entry in entries.Values)
      {
        // TryAdd: the file occasionally lists a symbol more than once; first entry wins, duplicates are ignored rather than fatal
        index.TryAdd(entry.Ticker, new CompanyIdentity(Company.NormalizeCik(entry.Cik.ToString()), entry.Title));
      }

      _byTicker = index;
      _expiresAtUtc = DateTime.UtcNow.Add(CacheLifetime);
      _logger.LogInformation("Loaded EDGAR company dir: {Count} tickers", index.Count);
      return index;
    }
    finally
    {
      _refreshLock.Release();
    }
  }

  private sealed record CompanyTickerEntry(
    [property: JsonPropertyName("cik_str")] long Cik,
    [property: JsonPropertyName("ticker")] string Ticker,
    [property: JsonPropertyName("title")] string Title);
}

// You can't lock across an await (the compiler refuses), so SemaphoreSlim(1,1) is the async mutex.
// The check-lock-recheck shape prevents a cache stampede: without it, ten concurrent resolves on a cold cache would fire ten 1 MB downloads.
// With it, one downloads while nine wait, then all ten read the result. Doesnt need chaching framework because its singleton