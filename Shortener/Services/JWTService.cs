using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Shortener.Entities;

namespace Shortener.Services;

public class JwtService
{
    public string GenerateJwtToken(User user)
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Login) };
        var jwt = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Program.SECRET_KEY)), SecurityAlgorithms.HmacSha256)
        );
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}