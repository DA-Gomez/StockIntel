namespace StockIntel.Api.Workers;

public class IngestionOptions
{
  public const string SectionName = "Ingestion";

  //kill switch (disable ingestion without redeploy)
  public bool Enabled { get; set; } = true;

  public TimeSpan Interval { get; set; } = TimeSpan.FromHours(1);
}