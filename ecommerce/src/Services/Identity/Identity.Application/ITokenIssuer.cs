using Identity.Domain;

namespace Identity.Application;

public interface ITokenIssuer
{
    IssuedToken Issue(IdentityAccount account);
}

public sealed record IssuedToken(string AccessToken, DateTimeOffset ExpiresAt);
