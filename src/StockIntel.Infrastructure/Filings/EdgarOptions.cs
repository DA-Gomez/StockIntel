using System.ComponentModel.DataAnnotations;

namespace StockIntel.Infrastructure.Filings;

public class EdgarOptions
{
  public const string SectionName = "Edgar";
  
  // SEC-required identification: "Product/Version (contact@email)"
  [Required(AllowEmptyStrings = false)]
  public string UserAgent { get; set; } = string.Empty;

  // EDGAR allows 10 requests/sec, we add a client side ceiling of half that
  [Range(1, 10)]
  public int RequestPerSecond { get; set; } = 5;
}