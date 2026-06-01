using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using StockIntel.Application.Abstractions.Persistence;
using StockIntel.Application.Abstractions.Security;
using StockIntel.Application.Users.Register;
using StockIntel.Domain.Entities;

namespace StockIntel.Application.UnitTests.Users;

public class RegisterUserHandlerTests
{
  //these are usually the dependencies. Substitute.For<T>() creates a mock of interface T that you can configure and verify
  private readonly IUserRepository _users = Substitute.For<IUserRepository>();
  private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
  private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
  private readonly IValidator<RegisterUserCommand> _validator = Substitute.For<IValidator<RegisterUserCommand>>();
  private RegisterUserHandler _handler;

  public RegisterUserHandlerTests()
  {
    _handler = new RegisterUserHandler(_users, _unitOfWork, _passwordHasher, _validator);

    _validator.ValidateAsync(Arg.Any<RegisterUserCommand>(), Arg.Any<CancellationToken>())
      .Returns(new ValidationResult());

    _passwordHasher.Hash(Arg.Any<string>()).Returns("hashed-password");
  }

  [Fact]
  public async Task HandleAsync_WithNewEmail_CreatesUserAndSaves()
  {
    var command = new RegisterUserCommand("test@example.com", "password");
    _users.EmailExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

    var result = await _handler.HandleAsync(command, CancellationToken.None);

    result.Email.Should().Be("test@example.com");
    result.UserId.Should().NotBeEmpty();
    await _users.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task HandleAsync_WithExistingEmail_ThrowsAndDoesNotSave()
  {
    var command = new RegisterUserCommand("test@example.com", "password123");
    _users.EmailExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

    var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

    await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
    await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task HandleAsync_WithValidationFailure_Throws()
  {
    var command = new RegisterUserCommand("bad-email", "xxx");
    var failures = new List<ValidationFailure>
    {
      new("Email", "Email must be a valid email address")
    };
    _validator.ValidateAsync(Arg.Any<RegisterUserCommand>(), Arg.Any<CancellationToken>())
      .Returns(new ValidationResult(failures));

    var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

    await act.Should().ThrowAsync<ValidationException>();
  }

  [Fact]
  public async Task HandleAsync_HashesPasswordBeforeString()
  {
    var command = new RegisterUserCommand("test@example.com", "plaintext-password");
    _users.EmailExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

    await _handler.HandleAsync(command, CancellationToken.None);

    _passwordHasher.Received(1).Hash("plaintext-password");
    await _users.Received(1).AddAsync(Arg.Is<User>(u => u.PasswordHash == "hashed-password"), Arg.Any<CancellationToken>());
  }
}

//mock.Method(args).Returns(value) sets up a return value when the method is called with matching arguments