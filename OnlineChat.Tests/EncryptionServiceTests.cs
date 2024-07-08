using Microsoft.Extensions.Configuration;

namespace OnlineChat.Tests
{
    public class EncryptionServiceTests
    {
        private readonly EncryptionService _encryptionService;

        public EncryptionServiceTests()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json")
                .Build();

            IConfigurationSection encryptionSettings = configuration.GetSection("EncryptionSettings");
            string? key = encryptionSettings["Key"];
            string? iv = encryptionSettings["IV"];

            _encryptionService = new EncryptionService(key, iv);
        }

        [Fact]
        public void HashPassword_ReturnsNonNullValues()
        {
            // Arrange
            string? password = "password";

            // Act
            (string hashedPassword, string salt) = _encryptionService.HashPassword(password);

            // Assert
            Assert.NotNull(hashedPassword);
            Assert.NotNull(salt);
        }

        [Fact]
        public void VerifyPassword_ReturnsTrueForCorrectPassword()
        {
            // Arrange
            string? password = "password";
            (string hashedPassword, string salt) = _encryptionService.HashPassword(password);

            // Act
            bool result = _encryptionService.VerifyPassword(password, hashedPassword, salt);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EncryptString_ReturnsEncryptedString()
        {
            // Arrange
            string? plainText = "Hello, World!";

            // Act
            string? encryptedString = _encryptionService.EncryptString(plainText);

            // Assert
            Assert.NotEmpty(encryptedString);
            Assert.NotEqual(plainText, encryptedString);
        }

        [Fact]
        public void DecryptString_ReturnsDecryptedString()
        {
            // Arrange
            string? plainText = "Hello, World!";
            string? encryptedString = _encryptionService.EncryptString(plainText);

            // Act
            string? decryptedString = _encryptionService.DecryptString(encryptedString);

            // Assert
            Assert.Equal(plainText, decryptedString);
        }
    }
}
