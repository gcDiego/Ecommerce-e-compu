using System.IdentityModel.Tokens.Jwt;
using Identity.Domain;
using Identity.Infrastructure;

namespace Identity.UnitTests;

public sealed class JwtTokenIssuerTests
{
    [Fact]
    public void Issue_UsesInteroperableRoleClaimName()
    {
        var issuer = new JwtTokenIssuer(
            "Ecommerce.Identity",
            "Ecommerce.Services",
            "test-signing-key-with-at-least-32-characters",
            TimeSpan.FromMinutes(5));
        var account = new IdentityAccount(
            7, "Ana", "Pérez", "ana@test.com", "hash", false, true, AccountType.Customer);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(issuer.Issue(account).AccessToken);

        Assert.Equal("Customer", token.Claims.Single(claim => claim.Type == "role").Value);
        Assert.DoesNotContain(token.Claims, claim => claim.Type.Contains("schemas.microsoft.com"));
    }
}
