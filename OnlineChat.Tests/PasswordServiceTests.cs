using OnlineChat.Services;

namespace OnlineChat.Tests
{
    public class PasswordServiceTests
    {
        [Fact]
        public void HashPassword_ReturnsNonNullValues()
        {
            // Arrange
            PasswordService service = new();
            string password = "password";

            // Act
            (string hashedPassword, string salt) = service.HashPassword(password);

            // Assert
            Assert.NotNull(hashedPassword);
            Assert.NotNull(salt);
        }

        [Fact]
        public void VerifyPassword_ReturnsTrueForCorrectPassword()
        {
            // Arrange
            PasswordService service = new();
            string password = "password";
            (string hashedPassword, string salt) = service.HashPassword(password);

            // Act
            bool result = service.VerifyPassword(password, hashedPassword, salt);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EncryptString_ReturnsEncryptedString()
        {
            // Arrange
            PasswordService service = new();
            string plainText = "Hello, World!";

            // Act
            string encryptedString = service.EncryptString(plainText);

            // Assert
            Assert.NotEmpty(encryptedString);
            Assert.NotEqual(plainText, encryptedString);
        }

        [Fact]
        public void DecryptString_ReturnsDecryptedString()
        {
            // Arrange
            PasswordService service = new();
            string plainText = "Hello, World!";
            string encryptedString = service.EncryptString(plainText);

            // Act
            string decryptedString = service.DecryptString(encryptedString);

            // Assert
            Assert.Equal(plainText, decryptedString);
        }
    }
}
