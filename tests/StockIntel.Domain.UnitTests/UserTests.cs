using FluentAssertions;
using StockIntel.Domain.Users;

namespace StockIntel.Domain.UnitTests;

public class UserTests
{
  [Fact]
  public void Register_WithValidInput_CreatesUser()
  {
    var user = User.Register("test@example.com", "hashed");

    user.Id.Should().NotBeEmpty();
    user.Email.Should().Be("test@example.com");
    user.PasswordHash.Should().Be("hashed");
    user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData(null)]
  public void Register_WithBlankEmail_Throws(string? email)
  {
    //act is a fn with no params. ! -> null forgiving operator (tell compiler to treat as non null)
    var act = () => User.Register(email!, "hashed");

    act.Should().Throw<ArgumentException>().WithMessage("*Email*");
  }

  [Fact]
  public void Register_WithoutAtSign_Throws()
  {
    var act = () => User.Register("notanemail", "hashed");

    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void Register_WithEmptyPasswordHash_Throws()
  {
    var act = () => User.Register("test@example.com", "");

    act.Should().Throw<ArgumentException>().WithMessage("*Password*");
  }
}