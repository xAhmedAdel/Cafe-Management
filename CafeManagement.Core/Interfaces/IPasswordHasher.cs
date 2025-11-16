namespace CafeManagement.Core.Interfaces;

public interface IPasswordHasher<T>
{
    string HashPassword(T user, string password);
    PasswordVerificationResult VerifyHashedPassword(T user, string hashedPassword, string providedPassword);
}

public enum PasswordVerificationResult
{
    Failed,
    Success,
    SuccessRehashNeeded
}