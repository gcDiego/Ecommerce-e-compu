using Identity.Domain;

namespace Identity.Application;

public sealed class AuthenticationService(
    IIdentityRepository repository,
    IPasswordVerifier passwordVerifier,
    ITokenIssuer tokenIssuer)
{
    public async Task<AuthenticationResult?> AuthenticateAsync(
        string email,
        string password,
        AccountType accountType,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var account = await repository.FindByEmailAsync(normalizedEmail, accountType, cancellationToken);
        if (account is null || !account.IsActive)
        {
            return null;
        }

        var verification = passwordVerifier.Verify(password, account.PasswordHash);
        if (verification == PasswordVerificationOutcome.Failed)
        {
            return null;
        }

        if (verification == PasswordVerificationOutcome.SuccessRehashNeeded)
        {
            await repository.UpdatePasswordHashAsync(
                account.Id,
                account.Type,
                account.PasswordHash,
                passwordVerifier.Hash(password),
                cancellationToken);
        }

        var token = tokenIssuer.Issue(account);
        return new AuthenticationResult(
            account.Id,
            account.FirstName,
            account.LastName,
            account.Email,
            account.Type,
            account.MustResetPassword,
            token.AccessToken,
            token.ExpiresAt);
    }
}

public sealed record AuthenticationResult(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    AccountType AccountType,
    bool MustResetPassword,
    string AccessToken,
    DateTimeOffset ExpiresAt);
