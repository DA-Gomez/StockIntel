using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32.SafeHandles;
using StockIntel.Application.Abstractions.Security;
using StockIntel.Domain.Entities;

namespace StockIntel.Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
  private readonly JwtOptions _options;

  public JwtTokenGenerator(IOptions<JwtOptions> options)
  {
    _options = options.Value;
  }

  public string GerateToken(User user)
  {
    var claims = new List<Claim> //claim is a piece of info about the user
    { 
      //same syntax as new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()); it already knows its type Claim
      new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
      new(JwtRegisteredClaimNames.Email, user.Email),
      new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
      issuer: _options.Issuer,
      audience: _options.Audience,
      claims: claims,
      expires: DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes),
      signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}

//ASP.net uses the Options Pattern (IOptions) to help us load configs like the one we need (issuer, audience ...) using JwtOptions

//JwtRegisteredClaimNames -> sub (Subjet, unique id of the user), email (email), jti (created a new identifier for the token)
