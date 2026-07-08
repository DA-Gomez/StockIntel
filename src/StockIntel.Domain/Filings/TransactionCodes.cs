namespace StockIntel.Domain.Filings;


//interpretation of SEC transactions code
public static class TransactionCodes
{
  public static bool IsOpenMarketPurchase(string code, bool isAcquisition)
    => code == "P" && isAcquisition;

  public static bool IsOpenMarketSale(string code, bool isAcquisition)
    => code == "S" && !isAcquisition;
}