using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GraduacionUni.Application.Services;
using GraduacionUni.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GraduacionUni.Infrastructure.Security;

public sealed class TokenService(IOptions<JwtOptions> options) : ITokenService
{
    public string CreateToken(User user)
    {
        var jwt = options.Value;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Nombres de claim "planos" (sub, name, role) para no depender del mapeo automático de .NET.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("name", user.Email),
            new Claim("role", user.Role)
        };

        var now = DateTime.UtcNow;

        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(jwt.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
