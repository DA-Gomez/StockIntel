using StockIntel.Application.Abstractions.Filings;
using StockIntel.Domain.Common;

namespace StockIntel.Api.IntegrationTests.TestDoubles;

// Resolves every ticker to one fixed identity (Apple by default). Pass cik/name to
// impersonate a different company, or null to model an unknown ticker.
public sealed class StubDirectory : ICompanyDirectory
{
  private readonly CompanyIdentity? _identity;

  public StubDirectory(string? cik = "0000320193", string name = "Apple Inc.")
    => _identity = cik is null ? null : new CompanyIdentity(cik, name);

  public Task<CompanyIdentity?> ResolveAsync(Ticker ticker, CancellationToken cancellationToken)
    => Task.FromResult(_identity);
}
