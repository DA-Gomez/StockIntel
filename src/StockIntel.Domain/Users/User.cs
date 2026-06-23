namespace StockIntel.Domain.Users;

public class User
{
  public Guid Id { get; private set; }
  public string Email { get; private set; } = string.Empty;
  public string PasswordHash { get; private set; } = string.Empty;
  public DateTime CreatedAt { get; private set; }

  private User() { } // ef core

  public static User Register(string email, string passwordHash)
  {
    if (string.IsNullOrWhiteSpace(email))
      throw new ArgumentException("Email is required", nameof(email));
    if (!email.Contains('@'))
      throw new ArgumentException("Email must contain '@'", nameof(email));
    if (string.IsNullOrWhiteSpace(passwordHash))
      throw new ArgumentException("Password hash is required", nameof(passwordHash));
    
    return new User
    {
      Id = Guid.NewGuid(),
      Email = email,
      PasswordHash = passwordHash,
      CreatedAt = DateTime.UtcNow,
    };
  }
}