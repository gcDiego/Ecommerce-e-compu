namespace Identity.Domain;

public enum AccountType
{
    Customer,
    Administrator
}

public sealed record IdentityAccount(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string PasswordHash,
    bool MustResetPassword,
    bool IsActive,
    AccountType Type);
