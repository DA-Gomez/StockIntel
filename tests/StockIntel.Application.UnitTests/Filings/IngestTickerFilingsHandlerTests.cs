using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using StockIntel.Application.Abstractions.Filings;
using StockIntel.Application.Abstractions.Persistence;
using StockIntel.Application.Filings.IngestTickerFilings;
using StockIntel.Domain.Common;
using StockIntel.Domain.Filings;

namespace StockIntel.Application.UnitTests.Filings;

public class IngestTickerFilingsHandlerTests
{
  private readonly ICompanyDirectory _directory = Substitute.For<ICompanyDirectory>();
  private readonly IInsiderFilingSource _source = Substitute.For<IInsiderFilingSource>();
  private readonly ICompanyRepository _companies = Substitute.For<ICompanyRepository>();
  private readonly IInsiderFilingRepository _filings = Substitute.For<IInsiderFilingRepository>();
  private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
  private readonly IngestTickerFilingsHandler _handler;

  public IngestTickerFilingsHandlerTests()
  {
    _handler = new IngestTickerFilingsHandler(_directory, _source, _companies, _filings,
      _unitOfWork, NullLogger<IngestTickerFilingsHandler>.Instance);
  }

  [Fact]
  public async Task Unknown_ticker_returns_zero_without_touching_edgar()
  {
    _directory.ResolveAsync(Arg.Any<Ticker>(), Arg.Any<CancellationToken>()).Returns((CompanyIdentity?)null);

    var res = await _handler.HandleAsync(new IngestTickerFilingsCommand("ZZZ"), default);

    res.Should().Be(0);
    await _source.DidNotReceiveWithAnyArgs().GetRecentForm4ReferencesAsync(default!, default);
  }

  [Fact]
  public async Task Already_stored_filings_are_not_fetched_again()
  {
    var (known, fresh) = (Reference("0001214156-26-000001"), Reference("0001214156-26-000002"));
    SetUpHappyPath(references: new[] { known, fresh });

    _filings.ExistsAsync(known.AccessionNumber, Arg.Any<CancellationToken>()).Returns(true);
    _filings.ExistsAsync(fresh.AccessionNumber, Arg.Any<CancellationToken>()).Returns(false);
    _filings.TryAddAsync(Arg.Any<InsiderFiling>(), Arg.Any<CancellationToken>()).Returns(true);

    var res = await _handler.HandleAsync(new IngestTickerFilingsCommand("AAPL"), default);

    // the fresh filing is ingested; the already-stored one is skipped
    res.Should().Be(1);

    // the whole point: a filing we already hold is never fetched/parsed again
    await _source.DidNotReceive().GetFilingAsync(known, Arg.Any<CancellationToken>());
    await _source.Received(1).GetFilingAsync(fresh, Arg.Any<CancellationToken>());
  }

  // helpers ----------------

  private void SetUpHappyPath(FilingReference[] references)
  {
    _directory.ResolveAsync(Arg.Any<Ticker>(), Arg.Any<CancellationToken>())
      .Returns(new CompanyIdentity("0000320193", "Apple Inc."));

    _companies.GetByCikAsync("0000320193", Arg.Any<CancellationToken>())
      .Returns(Company.Create("320193", "Apple Inc."));

    _source.GetRecentForm4ReferencesAsync("0000320193", Arg.Any<CancellationToken>())
      .Returns(references);

    _source.GetFilingAsync(Arg.Any<FilingReference>(), Arg.Any<CancellationToken>())
      .Returns(CreateParsed());
  }

  private static FilingReference Reference(string accession) =>
    new("0000320193", accession, new DateOnly(2026, 4, 3), "wk-form4_1.xml");

  private static ParsedForm4 CreateParsed() =>
    new("0000320193", "0001214156", "COOK TIMOTHY D",
      IsDirector: true,
      IsOfficer: true,
      IsTenPercentOwner: false,
      OfficerTitle: "Chief Executive Officer",
      Transactions: new[]
      {
        new ParsedForm4Transaction(new DateOnly(2026, 4, 2), "S", 100000m, 171.9412m,
          IsAcquisition: false, SharesOwnedAfter: 3380180m, IsDirectOwnership: true)
      });
}