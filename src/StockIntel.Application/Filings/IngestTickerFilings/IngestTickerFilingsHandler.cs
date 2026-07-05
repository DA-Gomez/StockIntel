using StockIntel.Application.Abstractions.Filings;
using StockIntel.Application.Abstractions.Persistence;
using StockIntel.Application.Common;
using Microsoft.Extensions.Logging;
using StockIntel.Domain.Common;
using StockIntel.Domain.Filings;


namespace StockIntel.Application.Filings.IngestTickerFilings;

public class IngestTickerFilingsHandler : ICommandHandler<IngestTickerFilingsCommand, int>
{
  private readonly ICompanyDirectory _directory;
  private readonly IInsiderFilingSource _source;
  private readonly ICompanyRepository _companies;
  private readonly IInsiderFilingRepository _filings;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<IngestTickerFilingsHandler> _logger;

  public IngestTickerFilingsHandler(
      ICompanyDirectory directory,
      IInsiderFilingSource source,
      ICompanyRepository companies,
      IInsiderFilingRepository filings,
      IUnitOfWork unitOfWork,
      ILogger<IngestTickerFilingsHandler> logger)
  {
      _directory = directory;
      _source = source;
      _companies = companies;
      _filings = filings;
      _unitOfWork = unitOfWork;
      _logger = logger;
  }

  public async Task<int> HandleAsync(IngestTickerFilingsCommand command, CancellationToken cancellationToken)
  {
    var ticker = Ticker.Create(command.Symbol);

    var identity = await _directory.ResolveAsync(ticker, cancellationToken);
    if (identity is null)
    {
      //users can watch (not every ticker is a filing company)
      _logger.LogWarning("Ticker {Ticker} not found in the edgar directory. skipping", ticker.Symbol);
      return 0;
    }

    //ensure the company exists and knows this ticker
    var company = await _companies.GetByCikAsync(identity.Cik, cancellationToken);
    if (company is null)
    {
      company = Company.Create(identity.Cik, identity.Name);
      await _companies.AddAsync(company, cancellationToken);
    }
    company.AddTickerListening(ticker);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    var references = await _source.GetRecentForm4ReferencesAsync(identity.Cik, cancellationToken);

    var added = 0;
    foreach (var reference in references)
    {
      cancellationToken.ThrowIfCancellationRequested();

      //skip the expensive part (fetch + parse) for filings we already hold. The unique index remains the guarantee
      if (await _filings.ExistsAsync(reference.AccessionNumber,cancellationToken)) continue;

      var parsed = await _source.GetFilingAsync(reference, cancellationToken);

      var filing = InsiderFiling.Create(
        company.Id,
        reference.AccessionNumber,
        reference.FilingDate,
        parsed.InsiderCik,
        parsed.InsiderName,
        parsed.IsDirector,
        parsed.IsOfficer,
        parsed.IsTenPercentOwner,
        parsed.OfficerTitle);

      //0 transactions is fine: the filing row itself is the "seen" marker that stops re-fetching 
      foreach (var t in parsed.Transactions)
      {
        filing.AddTransaction(t.TransactionDate, t.Code, t.Shares, t.PricePerShare, t.IsAcquisition, t.SharesOwnedAfter, t.IsDirectOwnership);
      }

      //losing the insert rate (false) is success by other means (means its already created)
      if (await _filings.TryAddAsync(filing, cancellationToken)) added++;
    }

    if (added > 0)
      _logger.LogInformation("ingested {Count} new Form 4 filings for {Ticker}", added, ticker.Symbol);

    return added;
  }
}