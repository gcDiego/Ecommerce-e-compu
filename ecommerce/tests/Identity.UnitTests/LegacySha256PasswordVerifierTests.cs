using System.Security.Cryptography;
using System.Text;
using Identity.Application;
using Identity.Infrastructure;

namespace Identity.UnitTests;

public sealed class LegacySha256PasswordVerifierTests
{
    [Fact]
    public void Verify_WithLegacyHashAndCorrectPassword_RequestsRehash()
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("correcta"))).ToLowerInvariant();

        Assert.Equal(
            PasswordVerificationOutcome.SuccessRehashNeeded,
            new LegacySha256PasswordVerifier().Verify("correcta", hash));
    }

    [Fact]
    public void Verify_WithMalformedHash_Fails()
    {
        Assert.Equal(
            PasswordVerificationOutcome.Failed,
            new LegacySha256PasswordVerifier().Verify("correcta", "no-es-hexadecimal"));
    }

    [Fact]
    public void Hash_ThenVerify_UsesAdaptiveHash()
    {
        var verifier = new LegacySha256PasswordVerifier();
        var hash = verifier.Hash("Nueva-Password-123");

        Assert.InRange(hash.Length, 65, 150);
        Assert.Equal(PasswordVerificationOutcome.Success, verifier.Verify("Nueva-Password-123", hash));
        Assert.Equal(PasswordVerificationOutcome.Failed, verifier.Verify("incorrecta", hash));
    }
}
