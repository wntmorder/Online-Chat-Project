using System.Security.Cryptography;
using System.Text;

namespace OnlineChat.Services
{
    /// <summary>
    /// Service for password hashing and encryption/decryption operations.
    /// </summary>
    public class PasswordService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordService"/> class with specified encryption key and IV.
        /// </summary>
        /// <param name="key">Encryption key in hexadecimal format.</param>
        /// <param name="iv">Initialization vector in hexadecimal format.</param>
        public PasswordService(string key, string iv)
        {
            _key = Convert.FromHexString(key);
            _iv = Convert.FromHexString(iv);

            if (_key.Length != 16 || _iv.Length != 16)
            {
                throw new ArgumentException("Key and IV must be 16 bytes (128 bits) long.");
            }
        }

        /// <summary>
        /// Hashes the specified password using SHA-256 and generates a salt.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>A tuple containing the hashed password and the salt used for hashing.</returns>
        public (string hashedPassword, string salt) HashPassword(string password)
        {
            byte[] saltBytes = new byte[16];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            string salt = Convert.ToBase64String(saltBytes);

            using SHA256 sha256 = SHA256.Create();
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] saltedPasswordBytes = new byte[passwordBytes.Length + saltBytes.Length];
            Buffer.BlockCopy(passwordBytes, 0, saltedPasswordBytes, 0, passwordBytes.Length);
            Buffer.BlockCopy(saltBytes, 0, saltedPasswordBytes, passwordBytes.Length, saltBytes.Length);
            byte[] hashedPasswordBytes = sha256.ComputeHash(saltedPasswordBytes);
            string hashedPassword = Convert.ToBase64String(hashedPasswordBytes);

            return (hashedPassword, salt);
        }

        /// <summary>
        /// Verifies the specified password against the given hashed password and salt.
        /// </summary>
        /// <param name="password">The password to verify.</param>
        /// <param name="hashedPassword">The hashed password to verify against.</param>
        /// <param name="salt">The salt used for hashing the password.</param>
        /// <returns><c>true</c> if the password matches the hashed password; otherwise, <c>false</c>.</returns>
        public bool VerifyPassword(string password, string hashedPassword, string salt)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] saltBytes = Convert.FromBase64String(salt);
            byte[] saltedPasswordBytes = new byte[passwordBytes.Length + saltBytes.Length];
            Buffer.BlockCopy(passwordBytes, 0, saltedPasswordBytes, 0, passwordBytes.Length);
            Buffer.BlockCopy(saltBytes, 0, saltedPasswordBytes, passwordBytes.Length, saltBytes.Length);

            using SHA256 sha256 = SHA256.Create();
            byte[] hashedPasswordBytes = sha256.ComputeHash(saltedPasswordBytes);
            string hashedPasswordAttempt = Convert.ToBase64String(hashedPasswordBytes);

            return hashedPasswordAttempt == hashedPassword;
        }

        /// <summary>
        /// Encrypts the specified plain text using AES encryption.
        /// </summary>
        /// <param name="plainText">The plain text to encrypt.</param>
        /// <returns>The encrypted text in base64 format.</returns>
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

        /// <summary>
        /// Decrypts the specified cipher text using AES decryption.
        /// </summary>
        /// <param name="cipherText">The cipher text to decrypt.</param>
        /// <returns>The decrypted plain text.</returns>
        /// <exception cref="CryptographicException">Thrown when decryption fails.</exception>
        public string DecryptString(string cipherText)
        {
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

            try
            {
                using Aes aesAlg = Aes.Create();
                aesAlg.Key = _key;
                aesAlg.IV = _iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using MemoryStream msDecrypt = new(cipherTextBytes);
                using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
                using StreamReader srDecrypt = new(csDecrypt);

                return srDecrypt.ReadToEnd();
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine($"Decryption failed: {ex.Message}");
                throw;
            }
        }
    }
}
