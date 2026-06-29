using System.Security.Cryptography;

namespace DPWrestlingScoreboard.Services
{
    /// <summary>
    /// Хеширование паролей (PBKDF2). В БД не хранятся открытые пароли.
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;
        private const char Delimiter = ':';

        public static bool IsHashed(string? stored) =>
            !string.IsNullOrEmpty(stored) && stored.StartsWith("PBKDF2", StringComparison.Ordinal);

        public static string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var key = DeriveKey(password, salt);
            return $"PBKDF2{Delimiter}{Iterations}{Delimiter}{Convert.ToBase64String(salt)}{Delimiter}{Convert.ToBase64String(key)}";
        }

        public static bool Verify(string password, string stored)
        {
            if (!IsHashed(stored))
                return false;

            var parts = stored.Split(Delimiter);
            if (parts.Length != 4 || parts[0] != "PBKDF2" || !int.TryParse(parts[1], out int iterations))
                return false;

            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
            var actual = DeriveKey(password, salt, iterations);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }

        private static byte[] DeriveKey(string password, byte[] salt, int iterations = Iterations)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(KeySize);
        }
    }
}
