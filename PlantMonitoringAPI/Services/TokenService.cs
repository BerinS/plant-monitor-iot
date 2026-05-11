using System.Security.Cryptography;
using System.Text;

namespace PlantMonitoringAPI.Services
{
    public class TokenService
    {
        // generates 32-byte salt as a hex string
        public string GenerateSalt()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        // SHA256(plainToken + salt) — salt appended as suffix
        // matches EMQX salt_position = suffix config exactly
        public string HashToken(string plainToken, string salt)
        {
            var input = plainToken + salt;
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        // Used at auth time: hash what the device sent, compare to DB
        public bool VerifyToken(string plainToken, string storedHash, string storedSalt)
        {
            var hash = HashToken(plainToken, storedSalt);
            return hash == storedHash;
        }
    }
}