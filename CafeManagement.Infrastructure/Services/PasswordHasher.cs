using CafeManagement.Core.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace CafeManagement.Infrastructure.Services;

public class PasswordHasher<T> : IPasswordHasher<T>
{
    public string HashPassword(T user, string password)
    {
        byte[] salt;
        byte[] hash;
        using (var hmac = new HMACSHA512())
        {
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        var hashBytes = new byte[64 + 16];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 64);

        return Convert.ToBase64String(hashBytes);
    }

    public PasswordVerificationResult VerifyHashedPassword(T user, string hashedPassword, string providedPassword)
    {
        try
        {
            var hashBytes = Convert.FromBase64String(hashedPassword);
            if (hashBytes.Length != 80)
                return PasswordVerificationResult.Failed;

            var salt = new byte[16];
            var storedHash = new byte[64];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            Array.Copy(hashBytes, 16, storedHash, 0, 64);

            using (var hmac = new HMACSHA512(salt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(providedPassword));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i])
                        return PasswordVerificationResult.Failed;
                }
            }

            return PasswordVerificationResult.Success;
        }
        catch
        {
            return PasswordVerificationResult.Failed;
        }
    }
}