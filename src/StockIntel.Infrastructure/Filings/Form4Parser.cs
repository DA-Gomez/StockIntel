using System.Globalization;
using System.Xml.Linq;
using StockIntel.Application.Abstractions.Filings;

namespace StockIntel.Infrastructure.Filings;

internal static class Form4Parser
{
  public static ParsedForm4 Parse(string xml)
  {
    var root = XDocument.Parse(xml).Root
      ?? throw new FormatException("Empty XML");
    if (root.Name.LocalName != "ownershipDocument")
      throw new FormatException($"Expected ownershipDocument, found <{root.Name.LocalName}>");
    
    var issuerCik = Required(root.Element("issuer")?.Element("issuerCik"), "issuer/issuerCik");
  
    //a filing can list several reportingOwners. take first
    var owner = root.Element("reportingOwner")
      ?? throw new FormatException("Missing reportingOwner");
    var ownerId = owner.Element("reportingOwnerId");
    var relationship = owner.Element("reportingOwnerRelationship");

    var transactions = new List<ParsedForm4Transaction>();
    var table = root.Element("nonDerivativeTable");
    if (table is not null)
    {
      // By name: the table also contains nonDerivativeHolding elements
      // (position statements, no date, no code) we shuoldnt touch
      foreach (var tx in table.Elements("nonDerivativeTransaction"))
      {
        var amounts = tx.Element("transactionAmounts");
        transactions.Add(new ParsedForm4Transaction(
          TransactionDate:    DateOnly.Parse(Required(tx.Element("transactionDate"), "transactionDate"), CultureInfo.InvariantCulture),
          Code:               Required(tx.Element("transactionCoding")?.Element("transactionCode"), "transactionCode"),
          Shares:             RequiredDecimal(amounts?.Element("transactionShares"), "transactionShares"),
          PricePerShare:      OptionalDecimal(amounts?.Element("transactionPricePerShare")),
          IsAcquisition:      Required(amounts?.Element("transactionAcquiredDisposedCode"),"transactionAcquiredDisposedCode") == "A",
          SharesOwnedAfter:   OptionalDecimal(tx.Element("postTransactionAmounts")?.Element("sharesOwnedFollowingTransaction")),
          IsDirectOwnership: (Value(tx.Element("ownershipNature")?.Element("directOrIndirectOwnership")) ?? "D") == "D"));
      }
    }

    return new ParsedForm4(
      IssuerCik: issuerCik,
      InsiderCik: Required(ownerId?.Element("rptOwnerCik"), "rptOwnerCik"),
      InsiderName: Required(ownerId?.Element("rptOwnerName"), "rptOwnerName"),
      IsDirector: Flag(relationship?.Element("isDirector")),
      IsOfficer: Flag(relationship?.Element("isOfficer")),
      IsTenPercentOwner: Flag(relationship?.Element("isTenPercentOwner")),
      OfficerTitle: Value(relationship?.Element("officerTitle")),
      Transactions: transactions);
  }

  //helpers -----

  //absorbs edgars 3 leaf shapes: direct text, a &lt;value&gt; child,or empty/missing -> null
  private static string? Value(XElement? element)
  {
    if (element is null) return null;
    var text = element.Element("value")?.Value ?? element.Value;
    text = text.Trim();
    return text.Length == 0 ? null : text;
  }

  private static string Required(XElement? element, string name) =>
    Value(element) ?? throw new FormatException($"Missing required element {name}");

  private static decimal RequiredDecimal(XElement? element, string name) =>
    decimal.Parse(Required(element, name), System.Globalization.NumberStyles.Number, CultureInfo.InvariantCulture);
  
  private static decimal? OptionalDecimal(XElement? element)
  {
    var text = Value(element);
    return text is null ? null : decimal.Parse(text, NumberStyles.Number, CultureInfo.InvariantCulture);
  }

  //: "1"/"0" and "true"/"false" both happen
  private static bool Flag(XElement? element)
  {
    var value = Value(element);
    return value is "1" || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
  }
}