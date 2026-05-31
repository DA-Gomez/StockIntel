namespace StockIntel.Infrastructure.Security;

public class JwtOptions
{
  public const string SectionName = "Jwt";

  public string Issuer { get; init; } = string.Empty; //who issued the token. used in validation
  public string Audience { get; init; } = string.Empty; //who the token is inteded for
  public string SigningKey { get; init; } = string.Empty; //HMAC secret (long and high-entropy)
  public int ExpirationMinutes { get; init; } = 60;
}