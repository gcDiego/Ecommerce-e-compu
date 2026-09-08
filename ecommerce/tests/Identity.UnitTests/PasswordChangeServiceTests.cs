using Identity.Application;
using Identity.Domain;

namespace Identity.UnitTests;

public sealed class PasswordChangeServiceTests
{
    [Fact]
    public async Task ChangeAsync_WithValidCurrentPassword_StoresAdaptiveHash()
    {
        var account = new IdentityAccount(7, "Ana", "Pérez", "ana@test.com", "old-hash", false, true, AccountType.Customer);
        var repository = new StubRepository(account);
        var service = new PasswordChangeService(repository, new StubVerifier());

        var result = await service.ChangeAsync(
            7, AccountType.Customer, "actual", "Nueva-Clave-123!", CancellationToken.None);

        Assert.Equal(PasswordChangeOutcome.Success, result);
        Assert.Equal("new-adaptive-hash", repository.StoredHash);
        Assert.Equal(7, repository.UpdatedAccountId);
    }

    [Theory]
    [InlineData("corta")]
    [InlineData("sin-mayusculas-123!")]
    [InlineData("SIN-MINUSCULAS-123!")]
    [InlineData("SinNumerosClave!")]
    [InlineData("SinCaracterEspecial123")]
    public async Task ChangeAsync_WithWeakNewPassword_ReturnsInvalidRequest(string newPassword)
    {
        var service = new PasswordChangeService(new StubRepository(null), new StubVerifier());

        var result = await service.ChangeAsync(
            7, AccountType.Customer, "actual", newPassword, CancellationToken.None);

        Assert.Equal(PasswordChangeOutcome.InvalidRequest, result);
    }

    private sealed class StubRepository(IdentityAccount? account) : IIdentityRepository
    {
        public int UpdatedAccountId { get; private set; }
        public string? StoredHash { get; private set; }

        public Task<IdentityAccount?> FindByEmailAsync(string email, AccountType accountType, CancellationToken cancellationToken) => Task.FromResult(account);
        public Task<IdentityAccount?> FindByIdAsync(int id, AccountType accountType, CancellationToken cancellationToken) => Task.FromResult(account);
        public Task<bool> CanConnectAsync(CancellationToken cancellationToken) => Task.FromResult(true);

        public Task<bool> UpdatePasswordHashAsync(int id, AccountType accountType, string expectedCurrentHash, string newHash, CancellationToken cancellationToken)
        {
            UpdatedAccountId = id;
            StoredHash = newHash;
            return Task.FromResult(true);
        }
    }

    private sealed class StubVerifier : IPasswordVerifier
    {
        public PasswordVerificationOutcome Verify(string password, string storedHash) =>
            password == "actual" ? PasswordVerificationOutcome.Success : PasswordVerificationOutcome.Failed;

        public string Hash(string password) => "new-adaptive-hash";
    }
}
