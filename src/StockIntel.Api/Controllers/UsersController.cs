using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using StockIntel.Application.Common;
using StockIntel.Application.Users.Register;

namespace StockIntel.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
  private readonly ICommandHandler<RegisterUserCommand, RegisterUserResponse> _registerHandler;

  public UsersController(ICommandHandler<RegisterUserCommand, RegisterUserResponse> registerHandler)
  {
    _registerHandler = registerHandler;
  }

  [HttpPost("register")]
  [ProducesResponseType(StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status409Conflict)]
  public async Task<IActionResult> Register(
    [FromBody] RegisterUserRequest request, //comes as JSON
    CancellationToken cancellationToken)
  {
    try
    {
      var command = new RegisterUserCommand(request.Email, request.Password);
      var result = await _registerHandler.HandleAsync(command, cancellationToken);
      return CreatedAtAction(nameof(Register), new { id = result.UserId }, result);
    }
    catch (ValidationException e)
    {
      return BadRequest(new { errors = e.Errors.Select(e => e.ErrorMessage)});
    }
    catch (InvalidOperationException e)
    {
      return Conflict(new { error = e.Message });
    }
  }
}

public record RegisterUserRequest(string Email, string Password);