﻿using Microsoft.Extensions.Configuration;
using OnlineChat.Controllers;

namespace OnlineChat.Tests
{
    public class AccountControllerTests
    {
        private readonly PasswordService _passwordService;
        private readonly ChatDbContext _dbContext;

        public AccountControllerTests()
        {
            DbContextOptions<ChatDbContext> options = new DbContextOptionsBuilder<ChatDbContext>()
               .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
               .Options;

            _dbContext = new ChatDbContext(options);

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true)
                .Build();

            IConfigurationSection encryptionSettings = configuration.GetSection("EncryptionSettings");
            string? key = encryptionSettings["Key"];
            string? iv = encryptionSettings["IV"];

            _passwordService = new PasswordService(key, iv);
        }

        [Fact]
        public async Task Register_ValidModel_ReturnsOkResult()
        {
            AccountController controller = new(_dbContext, _passwordService);
            RegisterModel registerModel = new()
            {
                Email = "test@example.com",
                Username = "TestUser",
                Password = "TestPassword",
                ConfirmPassword = "TestPassword"
            };

            IActionResult result = await controller.Register(registerModel);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Login_ValidModel_ReturnsOkResult()
        {
            AccountController controller = new(_dbContext, _passwordService);
            (string hashedPassword, string salt) hashedPasswordResult = _passwordService.HashPassword("TestPassword");
            User user = new()
            {
                Email = "test@example.com",
                Username = "TestUser",
                PasswordHash = hashedPasswordResult.hashedPassword,
                Salt = hashedPasswordResult.salt
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            LoginModel loginModel = new()
            {
                EmailOrUsername = "test@example.com",
                Password = "TestPassword"
            };

            IActionResult result = await controller.Login(loginModel);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateAccount_ValidModel_ReturnsOkResult()
        {
            AccountController controller = new(_dbContext, _passwordService);
            (string hashedPassword, string salt) hashedPasswordResult = _passwordService.HashPassword("TestPassword");

            User user = new()
            {
                Email = "old@example.com",
                Username = "oldusername",
                PasswordHash = hashedPasswordResult.hashedPassword,
                Salt = hashedPasswordResult.salt
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            UpdateAccountModel model = new()
            {
                Email = "new@example.com",
                Username = "newusername",
                Password = "newpassword"
            };

            IActionResult result = await controller.UpdateAccount(user.UserId, model);

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal("new@example.com", user.Email);
            Assert.Equal("newusername", user.Username);
        }

        [Fact]
        public async Task DeleteUser_ExistingUserId_ReturnsOkResult()
        {
            // Arrange
            AccountController controller = new(_dbContext, _passwordService);
            (string hashedPassword, string salt) hashedPasswordResult = _passwordService.HashPassword("TestPassword");
            User user = new()
            {
                Email = "test@example.com",
                Username = "TestUser",
                PasswordHash = hashedPasswordResult.hashedPassword,
                Salt = hashedPasswordResult.salt
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            AccountController _controller = new AccountController(_dbContext, _passwordService);

            // Act
            IActionResult result = await _controller.DeleteUser(user.UserId);

            // Assert
            Assert.IsType<OkObjectResult>(result);

            // Verify user is deleted
            User? deletedUser = await _dbContext.Users.FindAsync(user.UserId);
            Assert.Null(deletedUser);
        }

        [Fact]
        public async Task DeleteUser_NonExistingUserId_ReturnsNotFoundResult()
        {
            // Arrange
            AccountController controller = new AccountController(_dbContext, _passwordService);

            // Act
            IActionResult result = await controller.DeleteUser("NonExistingUserId");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
