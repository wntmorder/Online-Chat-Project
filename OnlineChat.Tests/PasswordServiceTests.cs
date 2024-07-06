using Microsoft.Extensions.Configuration;

namespace OnlineChat.Tests
{
    public class PasswordServiceTests
    {
        private readonly PasswordService _passwordService;

        public PasswordServiceTests()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json")
                .Build();

            IConfigurationSection encryptionSettings = configuration.GetSection("EncryptionSettings");
            string? key = encryptionSettings["Key"];
            string? iv = encryptionSettings["IV"];

            _passwordService = new PasswordService(key, iv);
        }

        [Fact]
        public void HashPassword_ReturnsNonNullValues()
        {
            // Arrange
            string? password = "password";

            // Act
            (string hashedPassword, string salt) = _passwordService.HashPassword(password);

            // Assert
            Assert.NotNull(hashedPassword);
            Assert.NotNull(salt);
        }

        [Fact]
        public void VerifyPassword_ReturnsTrueForCorrectPassword()
        {
            // Arrange
            string? password = "password";
            (string hashedPassword, string salt) = _passwordService.HashPassword(password);

            // Act
            bool result = _passwordService.VerifyPassword(password, hashedPassword, salt);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EncryptString_ReturnsEncryptedString()
        {
            // Arrange
            string? plainText = "Hello, World!";

            // Act
            string? encryptedString = _passwordService.EncryptString(plainText);

            // Assert
            Assert.NotEmpty(encryptedString);
            Assert.NotEqual(plainText, encryptedString);
        }

        [Fact]
        public void DecryptString_ReturnsDecryptedString()
        {
            // Arrange
            string? plainText = "Hello, World!";
            string? encryptedString = _passwordService.EncryptString(plainText);

            // Act
            string? decryptedString = _passwordService.DecryptString(encryptedString);

            // Assert
            Assert.Equal(plainText, decryptedString);
        }
    }
}
