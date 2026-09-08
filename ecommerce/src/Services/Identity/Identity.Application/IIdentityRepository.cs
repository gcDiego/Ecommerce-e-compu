using Identity.Domain;

namespace Identity.Application;

public interface IIdentityRepository
{
    Task<IdentityAccount?> FindByEmailAsync(string email, AccountType accountType, CancellationToken cancellationToken);
    Task<IdentityAccount?> FindByIdAsync(int id, AccountType accountType, CancellationToken cancellationToken);
    Task<bool> UpdatePasswordHashAsync(
        int id,
        AccountType accountType,
        string expectedCurrentHash,
        string newHash,
        CancellationToken cancellationToken);
    Task<bool> CanConnectAsync(CancellationToken cancellationToken);
}
