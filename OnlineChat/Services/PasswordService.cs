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
            byte[] saltBytes = new byte[16];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            string salt = Convert.ToBase64String(saltBytes);

            using SHA256 sha256 = SHA256.Create();
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password + salt);
            byte[] hashedPasswordBytes = sha256.ComputeHash(passwordBytes);
            string hashedPassword = Convert.ToBase64String(hashedPasswordBytes);

            return (hashedPassword, salt);
        }

        public bool VerifyPassword(string password, string hashedPassword, string salt)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password + salt);
            byte[] hashedPasswordBytes = sha256.ComputeHash(passwordBytes);
            string hashedPasswordAttempt = Convert.ToBase64String(hashedPasswordBytes);

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
