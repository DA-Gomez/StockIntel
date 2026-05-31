using System.Security.Claims;
using StockIntel.Application.Abstractions.Authentication;

namespace StockIntel.Api.Authentication;

public class CurrentUser : ICurrentUser
{
  private readonly IHttpContextAccessor _httpContextAccessor;

  public CurrentUser(IHttpContextAccessor httpContextAccessor)
  {
    _httpContextAccessor = httpContextAccessor;
  }

  public Guid? UserId
  { //property, not a method (currentUser.UserId)
    get
    {
      var sub = _httpContextAccessor.HttpContext?.User
        .FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(sub, out var id) ? id : null;
    }
  }

  public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}


//When a controller is hit with a valid JWT, ASP.NET Core populates HttpContext.User with the claims from the token.
//But Application layer code shouldn't depend on HttpContext (Infrastructure/API concept)

//out var id -> Create a variable named id and let the method fill it