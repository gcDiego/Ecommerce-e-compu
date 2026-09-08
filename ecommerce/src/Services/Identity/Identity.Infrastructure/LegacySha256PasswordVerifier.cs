using System.Security.Cryptography;
using System.Text;
using Identity.Application;
using Microsoft.AspNetCore.Identity;

namespace Identity.Infrastructure;

public sealed class LegacySha256PasswordVerifier : IPasswordVerifier
{
    private static readonly PasswordHasher<object> AdaptiveHasher = new();
    private static readonly object HasherUser = new();

    public PasswordVerificationOutcome Verify(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(storedHash))
        {
            return PasswordVerificationOutcome.Failed;
        }

        if (IsLegacySha256(storedHash))
        {
            var computedHash = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            var storedBytes = Convert.FromHexString(storedHash);
            return CryptographicOperations.FixedTimeEquals(computedHash, storedBytes)
                ? PasswordVerificationOutcome.SuccessRehashNeeded
                : PasswordVerificationOutcome.Failed;
        }

        try
        {
            var result = AdaptiveHasher.VerifyHashedPassword(HasherUser, storedHash, password);
            return result switch
            {
                PasswordVerificationResult.Success => PasswordVerificationOutcome.Success,
                PasswordVerificationResult.SuccessRehashNeeded => PasswordVerificationOutcome.SuccessRehashNeeded,
                _ => PasswordVerificationOutcome.Failed
            };
        }
        catch (FormatException)
        {
            return PasswordVerificationOutcome.Failed;
        }
    }

    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("La contraseña no puede estar vacía.", nameof(password));
        }

        return AdaptiveHasher.HashPassword(HasherUser, password);
    }

    private static bool IsLegacySha256(string storedHash)
    {
        if (storedHash.Length != 64)
        {
            return false;
        }

        foreach (var character in storedHash)
        {
            if (!Uri.IsHexDigit(character))
            {
                return false;
            }
        }

        return true;
    }
}
