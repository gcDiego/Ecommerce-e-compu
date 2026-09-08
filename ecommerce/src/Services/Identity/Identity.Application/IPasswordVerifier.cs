namespace Identity.Application;

public enum PasswordVerificationOutcome
{
    Failed,
    Success,
    SuccessRehashNeeded
}

public interface IPasswordVerifier
{
    PasswordVerificationOutcome Verify(string password, string storedHash);
    string Hash(string password);
}
