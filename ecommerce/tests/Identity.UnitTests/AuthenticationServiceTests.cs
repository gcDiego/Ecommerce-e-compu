using Identity.Application;
using Identity.Domain;

namespace Identity.UnitTests;

public sealed class AuthenticationServiceTests
{
    [Fact]
    public async Task AuthenticateAsync_WithValidActiveAccount_ReturnsTokenWithoutHash()
    {
        var account = new IdentityAccount(4, "Ana", "Pérez", "ana@test.com", "hash", false, true, AccountType.Customer);
        var service = new AuthenticationService(new StubRepository(account), new StubVerifier(PasswordVerificationOutcome.Success), new StubTokenIssuer());

        var result = await service.AuthenticateAsync(" ANA@Test.com ", "secret", AccountType.Customer, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("token", result.AccessToken);
        Assert.Equal("ana@test.com", result.Email);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task AuthenticateAsync_WithInvalidPasswordOrInactiveAccount_ReturnsNull(bool passwordIsValid, bool isActive)
    {
        var account = new IdentityAccount(4, "Ana", "Pérez", "ana@test.com", "hash", false, isActive, AccountType.Administrator);
        var outcome = passwordIsValid ? PasswordVerificationOutcome.Success : PasswordVerificationOutcome.Failed;
        var service = new AuthenticationService(new StubRepository(account), new StubVerifier(outcome), new StubTokenIssuer());

        var result = await service.AuthenticateAsync("ana@test.com", "secret", AccountType.Administrator, CancellationToken.None);

        Assert.Null(result);
    }

    private sealed class StubRepository(IdentityAccount? account) : IIdentityRepository
    {
        public Task<IdentityAccount?> FindByEmailAsync(string email, AccountType accountType, CancellationToken cancellationToken) => Task.FromResult(account);
        public Task<IdentityAccount?> FindByIdAsync(int id, AccountType accountType, CancellationToken cancellationToken) => Task.FromResult(account);
        public Task<bool> UpdatePasswordHashAsync(int id, AccountType accountType, string expectedCurrentHash, string newHash, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<bool> CanConnectAsync(CancellationToken cancellationToken) => Task.FromResult(true);
    }

    private sealed class StubVerifier(PasswordVerificationOutcome result) : IPasswordVerifier
    {
        public PasswordVerificationOutcome Verify(string password, string storedHash) => result;
        public string Hash(string password) => "adaptive-hash";
    }

    private sealed class StubTokenIssuer : ITokenIssuer
    {
        public IssuedToken Issue(IdentityAccount account) => new("token", DateTimeOffset.UtcNow.AddMinutes(30));
    }
}
