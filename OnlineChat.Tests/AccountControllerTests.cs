using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineChat.Controllers;
using OnlineChat.Data;
using OnlineChat.Models;
using OnlineChat.Services;
using OnlineChat.ViewModels;
using System.Diagnostics;

namespace OnlineChat.Tests
{
    public class AccountControllerTests
    {
        [Fact]
        public async Task Register_ValidModel_ReturnsOkResult()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ChatDbContext>()
                .UseInMemoryDatabase(databaseName: "Register_ValidModel_ReturnsOkResult")
                .Options;
            using ChatDbContext dbContext = new(options);
            PasswordService passwordService = new();
            AccountController controller = new(dbContext, passwordService);
            RegisterModel registerModel = new()
            {
                Email = "test@example.com",
                Username = "TestUser",
                Password = "TestPassword", 
                ConfirmPassword = "TestPassword"
            };

            // Act
            IActionResult result = await controller.Register(registerModel);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Login_ValidModel_ReturnsOkResult()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ChatDbContext>()
                .UseInMemoryDatabase(databaseName: "Login_ValidModel_ReturnsOkResult")
                .Options;
            using ChatDbContext dbContext = new(options);
            PasswordService passwordService = new();
            AccountController controller = new(dbContext, passwordService);
            (string hashedPassword, string salt) hashedPasswordResult = passwordService.HashPassword("TestPassword");
            User user = new()
            {
                Email = "test@example.com",
                Username = "TestUser",
                PasswordHash = hashedPasswordResult.hashedPassword,
                Salt = hashedPasswordResult.salt
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
            LoginModel loginModel = new()
            {
                EmailOrUsername = "test@example.com",
                Password = "TestPassword"
            };

            // Act
            IActionResult result = await controller.Login(loginModel);

            // Assert
            try
            {
                Assert.IsType<OkObjectResult>(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Assertion failed: {ex.Message}");
                throw;
            }
        }
    }
}
