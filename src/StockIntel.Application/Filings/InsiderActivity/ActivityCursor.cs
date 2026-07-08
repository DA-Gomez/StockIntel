using System.Text;

namespace StockIntel.Application.Filings.InsiderActivity;

//the opaque token: base64url over "{date}|{guid}" (+ and / not allowed for query strings)
public static class ActivityCursor
{
  public static string Encode(DateOnly transactionDate, Guid transactionId)
  {
    var raw = $"{transactionDate:yyyy-MM-dd}|{transactionId:N}";
    return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw))
      .TrimEnd('=').Replace('+', '-').Replace('/', '_');
  }

  public static bool TryDecode(string cursor, out DateOnly transactionDate, out Guid transactionId)
  {
    transactionDate = default;
    transactionId = default;
    try
    {
      var base64 = cursor.Replace('-', '+').Replace('_', '/');
      base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
      var raw = Encoding.UTF8.GetString(Convert.FromBase64String(base64));

      var parts = raw.Split('|');
      return parts.Length == 2
        && DateOnly.TryParseExact(parts[0], "yyyy-MM-dd", out transactionDate)
        && Guid.TryParseExact(parts[1], "N", out transactionId);
    }
    catch (FormatException)
    {
      return false; //tampered or garbase input is a caller error not exception
    }
  }
}