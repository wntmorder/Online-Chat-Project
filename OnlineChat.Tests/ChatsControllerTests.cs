using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineChat.Controllers;
using OnlineChat.Data;
using OnlineChat.Models;
using OnlineChat.ViewModels;

namespace OnlineChat.Tests
{
    public class ChatsControllerTests
    {
        [Fact]
        public async Task CreateChat_ValidModel_ReturnsCreatedAtActionResult()
        {
            // Arrange
            DbContextOptions<ChatDbContext> options = new DbContextOptionsBuilder<ChatDbContext>()
                .UseInMemoryDatabase(databaseName: "CreateChat_ValidModel_ReturnsCreatedAtActionResult")
                .Options;
            using ChatDbContext dbContext = new(options);
            ChatsController controller = new(dbContext);
            CreateChatModel model = new()
            {
                Title = "Test Chat"
            };

            // Act
            IActionResult result = await controller.CreateChat(model);

            // Assert
            CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Chat chat = Assert.IsType<Chat>(createdResult.Value);
            Assert.Equal(model.Title, chat.Title);
        }

        [Fact]
        public async Task GetChat_ExistingChatId_ReturnsOkObjectResult()
        {
            // Arrange
            DbContextOptions<ChatDbContext> options = new DbContextOptionsBuilder<ChatDbContext>()
                .UseInMemoryDatabase(databaseName: "GetChat_ExistingChatId_ReturnsOkObjectResult")
                .Options;
            using ChatDbContext dbContext = new(options);
            Chat chat = new()
            {
                Title = "Test Chat"
            };
            dbContext.Chats.Add(chat);
            await dbContext.SaveChangesAsync();

            ChatsController controller = new(dbContext);

            // Act
            IActionResult result = await controller.GetChat(chat.ChatId);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Chat returnedChat = Assert.IsType<Chat>(okResult.Value);
            Assert.Equal(chat.ChatId, returnedChat.ChatId);
            Assert.Equal(chat.Title, returnedChat.Title);
        }

        [Fact]
        public async Task GetChat_NonExistingChatId_ReturnsNotFoundResult()
        {
            // Arrange
            DbContextOptions<ChatDbContext> options = new DbContextOptionsBuilder<ChatDbContext>()
                .UseInMemoryDatabase(databaseName: "GetChat_NonExistingChatId_ReturnsNotFoundResult")
                .Options;
            using ChatDbContext dbContext = new(options);
            ChatsController controller = new(dbContext);

            // Act
            IActionResult result = await controller.GetChat("NonExistingChatId");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetAllChats_ReturnsOkResult()
        {
            // Arrange
            DbContextOptions<ChatDbContext> options = new DbContextOptionsBuilder<ChatDbContext>()
                .UseInMemoryDatabase(databaseName: "GetAllChats_ReturnsOkResult")
                .Options;
            using ChatDbContext dbContext = new(options);
            ChatsController controller = new(dbContext);

            dbContext.Chats.Add(new Chat { Title = "Test Chat" });
            await dbContext.SaveChangesAsync();

            // Act
            IActionResult result = await controller.GetAllChats();

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            List<Chat> chats = Assert.IsType<List<Chat>>(okResult.Value);
            Assert.Single(chats);
        }

        [Fact]
        public async Task GetChatDetails_ReturnsOkResult()
        {
            // Arrange
            DbContextOptions<ChatDbContext> options = new DbContextOptionsBuilder<ChatDbContext>()
                .UseInMemoryDatabase(databaseName: "GetChatDetails_ReturnsOkResult")
                .Options;
            using ChatDbContext dbContext = new(options);
            ChatsController controller = new(dbContext);

            Chat chat = new() { Title = "Test Chat", MembersCount = 5 };
            dbContext.Chats.Add(chat);
            await dbContext.SaveChangesAsync();

            // Act
            IActionResult result = await controller.GetChatDetails(chat.ChatId);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            ChatDetailsViewModel details = Assert.IsType<ChatDetailsViewModel>(okResult.Value);
            Assert.Equal("Test Chat", details.Title);
            Assert.Equal(5, details.MembersCount);
        }
    }
}
