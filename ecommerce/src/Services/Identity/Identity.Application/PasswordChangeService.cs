using Identity.Domain;

namespace Identity.Application;

public sealed class PasswordChangeService(
    IIdentityRepository repository,
    IPasswordVerifier passwordVerifier)
{
    public async Task<PasswordChangeOutcome> ChangeAsync(
        int accountId,
        AccountType accountType,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken)
    {
        if (accountId <= 0 || string.IsNullOrEmpty(currentPassword) || !IsAcceptable(newPassword))
        {
            return PasswordChangeOutcome.InvalidRequest;
        }

        var account = await repository.FindByIdAsync(accountId, accountType, cancellationToken);
        if (account is null || !account.IsActive ||
            passwordVerifier.Verify(currentPassword, account.PasswordHash) == PasswordVerificationOutcome.Failed)
        {
            return PasswordChangeOutcome.InvalidCredentials;
        }

        if (currentPassword == newPassword)
        {
            return PasswordChangeOutcome.PasswordReused;
        }

        var updated = await repository.UpdatePasswordHashAsync(
            account.Id,
            account.Type,
            account.PasswordHash,
            passwordVerifier.Hash(newPassword),
            cancellationToken);

        return updated ? PasswordChangeOutcome.Success : PasswordChangeOutcome.Conflict;
    }

    private static bool IsAcceptable(string password) =>
        !string.IsNullOrWhiteSpace(password) &&
        password.Length is >= 12 and <= 128 &&
        password.Any(char.IsUpper) &&
        password.Any(char.IsLower) &&
        password.Any(char.IsDigit) &&
        password.Any(character => !char.IsLetterOrDigit(character));
}

public enum PasswordChangeOutcome
{
    Success,
    InvalidRequest,
    InvalidCredentials,
    PasswordReused,
    Conflict
}
