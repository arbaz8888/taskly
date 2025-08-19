using System.Security.Cryptography;
using System.Text;

namespace Taskly.Api.Auth;

public static class PasswordHasher
{
    public static (string hash, string salt) Hash(string password)
    {
        // 16-byte salt
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var hash = HashWithSalt(password, saltBytes);
        return (Convert.ToHexString(hash), Convert.ToHexString(saltBytes));
    }

    public static bool Verify(string password, string hexSalt, string hexHash)
    {
        var saltBytes = Convert.FromHexString(hexSalt);
        var computed = HashWithSalt(password, saltBytes);
        return Convert.ToHexString(computed) == hexHash;
    }

    private static byte[] HashWithSalt(string password, byte[] salt)
    {
        // PBKDF2
        return Rfc2898DeriveBytes.Pbkdf2(
            password: password,
            salt: salt,
            iterations: 100_000,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: 32
        );
    }
}
