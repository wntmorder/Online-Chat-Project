using System.Text;
using System.Security.Cryptography;

namespace OnlineChat.Services
{
    public class PasswordService
    {
        private readonly byte[] _key = GenerateKey(16);
        private readonly byte[] _iv = GenerateIV(16);

        private static byte[] GenerateKey(int keySize)
        {
            byte[] key = new byte[keySize];
            RandomNumberGenerator.Fill(key);
            return key;
        }

        private static byte[] GenerateIV(int ivSize)
        {
            byte[] iv = new byte[ivSize];
            RandomNumberGenerator.Fill(iv);
            return iv;
        }

        public (string hashedPassword, string salt) HashPassword(string password)
        {
            // Generate a random salt
            byte[] saltBytes = new byte[16];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            string salt = Convert.ToBase64String(saltBytes);

            // Hash the password with the salt
            using SHA256 sha256 = SHA256.Create();
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] saltedPasswordBytes = new byte[passwordBytes.Length + saltBytes.Length];
            Buffer.BlockCopy(passwordBytes, 0, saltedPasswordBytes, 0, passwordBytes.Length);
            Buffer.BlockCopy(saltBytes, 0, saltedPasswordBytes, passwordBytes.Length, saltBytes.Length);
            byte[] hashedPasswordBytes = sha256.ComputeHash(saltedPasswordBytes);
            string hashedPassword = Convert.ToBase64String(hashedPasswordBytes);

            return (hashedPassword, salt);
        }

        public bool VerifyPassword(string password, string hashedPassword, string salt)
        {
            // Convert the provided password to bytes
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // Combine the password bytes with the salt bytes
            byte[] saltBytes = Convert.FromBase64String(salt);
            byte[] saltedPasswordBytes = new byte[passwordBytes.Length + saltBytes.Length];
            Buffer.BlockCopy(passwordBytes, 0, saltedPasswordBytes, 0, passwordBytes.Length);
            Buffer.BlockCopy(saltBytes, 0, saltedPasswordBytes, passwordBytes.Length, saltBytes.Length);

            // Compute the hash of the combined bytes
            using SHA256 sha256 = SHA256.Create();
            byte[] hashedPasswordBytes = sha256.ComputeHash(saltedPasswordBytes);
            string hashedPasswordAttempt = Convert.ToBase64String(hashedPasswordBytes);

            // Compare the computed hash with the stored hashed password
            return hashedPasswordAttempt == hashedPassword;
        }

        public string EncryptString(string plainText)
        {
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = _key;
            aesAlg.IV = _iv;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using MemoryStream msEncrypt = new();
            using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using StreamWriter swEncrypt = new(csEncrypt);
                swEncrypt.Write(plainText);
            }

            byte[] encrypted = msEncrypt.ToArray();

            return Convert.ToBase64String(encrypted);
        }

        public string DecryptString(string cipherText)
        {
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

            using Aes aesAlg = Aes.Create();
            aesAlg.Key = _key;
            aesAlg.IV = _iv;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using MemoryStream msDecrypt = new(cipherTextBytes);
            using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new(csDecrypt);

            return srDecrypt.ReadToEnd();
        }
    }
}
