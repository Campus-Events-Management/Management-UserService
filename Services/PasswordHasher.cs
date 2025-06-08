using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace EventManagement.UserService.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int IterationCount = 10000;
        private const int NumBytesRequested = 256 / 8;
        private const int SaltSize = 128 / 8;

        public string HashPassword(string password)
        {
            // Generate a random salt
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash the password with PBKDF2
            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: IterationCount,
                numBytesRequested: NumBytesRequested);

            // Combine the salt and hash into a single string for storage
            byte[] hashBytes = new byte[SaltSize + NumBytesRequested];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, NumBytesRequested);
            
            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            // Extract the salt and hash from the stored password hash
            byte[] hashBytes = Convert.FromBase64String(passwordHash);
            
            // Ensure the stored hash has the expected format
            if (hashBytes.Length != SaltSize + NumBytesRequested)
            {
                return false;
            }

            byte[] salt = new byte[SaltSize];
            byte[] storedHash = new byte[NumBytesRequested];
            
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);
            Array.Copy(hashBytes, SaltSize, storedHash, 0, NumBytesRequested);

            // Hash the input password with the extracted salt
            byte[] computedHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: IterationCount,
                numBytesRequested: NumBytesRequested);

            // Compare the computed hash with the stored hash
            return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
        }
    }
} 