namespace StockIntel.Domain.Filings;

public class InsiderFilings
{
  private readonly List<InsiderTransactions> _transactions = new();
  
  public Guid Id { get; private set; }
  public Guid CompanyId { get; private set; }

  //Canonical form: 0001214156-26-000043 (dashed) Globally unique in EDGAR
  public string AccessionNumber { get; private set; } = string.Empty;
  public DateOnly FilingDate { get; private set; }

  //reporting owner - denormalized (check schema)
  public string InsiderCik { get; private set; } = string.Empty;
  public string InsiderName { get; private set; } = string.Empty;
  public bool IsDirector { get; private set; }
  public bool IsOfficer { get; private set; }
  public bool IsTenPercentOwner { get; private set; }
  public string? OfficerTitle { get; private set; }

  public DateTime IngestedAt { get; private set; }
  public IReadOnlyCollection<InsiderTransactions> Transactions => _transactions;

  private InsiderFilings() {} //ef core

  public static InsiderFilings Create(
    Guid companyId,
    string accessionNumber,
    DateOnly filingDate,
    string insiderCik,
    string insiderName,
    bool isDirector,
    bool isOfficer,
    bool isTenPercentOwner,
    string? officerTitle)
  {
    if (companyId == Guid.Empty) 
      throw new ArgumentException("CompanyId is required", nameof(companyId));
    if (string.IsNullOrWhiteSpace(insiderName)) 
      throw new ArgumentException("InsiderName is required", nameof(insiderName));

    return new InsiderFilings
    {
      Id = Guid.NewGuid(),
      CompanyId = companyId,
      AccessionNumber = NormalizeAccessionNumber(accessionNumber),
      FilingDate = filingDate,
      InsiderCik = Company.NormalizeCik(insiderCik),
      InsiderName = insiderName.Trim(),
      IsDirector = isDirector,
      IsOfficer = isOfficer,
      IsTenPercentOwner = isTenPercentOwner,
      OfficerTitle = string.IsNullOrWhiteSpace(officerTitle)? null : officerTitle.Trim(),
      IngestedAt = DateTime.UtcNow
    };
  }

  // EDGAR shows accession numbers both dashed (0001214156-26-000043) and
  // undashed (000121415626000043) depending on the endpoint. One canonical
  // form in the database, or our unique index silently stops protecting us.
  public static string NormalizeAccessionNumber(string accessionNumber)
  {
    if (string.IsNullOrWhiteSpace(accessionNumber))
      throw new ArgumentException("Accession number is required", nameof(accessionNumber));

    var digits = accessionNumber.Trim().Replace("-", "");
    if (digits.Length != 18 || !digits.All(char.IsDigit))
      throw new ArgumentException($"{accessionNumber} is not a valid accession number", nameof(accessionNumber));
    
    return $"{digits[..10]}-{digits[10..12]}-{digits[12..]}";
  }

  public void AddTransaction(
    DateOnly transactionDate,
    string code,
    decimal shares,
    decimal? pricePerShare,
    bool isAcquisition,
    decimal? sharesOwnedAfter,
    bool isDirectOwnership)
  {
    _transactions.Add(new InsiderTransactions(
      Id, transactionDate, code, shares, pricePerShare,
      isAcquisition, sharesOwnedAfter, isDirectOwnership));
  }
}