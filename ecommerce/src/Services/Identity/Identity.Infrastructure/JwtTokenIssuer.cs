using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Identity.Application;
using Identity.Domain;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure;

public sealed class JwtTokenIssuer(string issuer, string audience, string signingKey, TimeSpan lifetime) : ITokenIssuer
{
    public IssuedToken Issue(IdentityAccount account)
    {
        var expiresAt = DateTimeOffset.UtcNow.Add(lifetime);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, account.Email),
            new Claim(ClaimTypes.Role, account.Type.ToString()),
            new Claim("must_reset_password", account.MustResetPassword.ToString().ToLowerInvariant())
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, audience, claims, expires: expiresAt.UtcDateTime, signingCredentials: credentials);
        return new IssuedToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
