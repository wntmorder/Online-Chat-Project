using Microsoft.Extensions.Configuration;

namespace OnlineChat.Tests
{
    public class ChatsControllerTests
    {
        private readonly PasswordService _passwordService;
        private readonly ChatDbContext _dbContext;

        public ChatsControllerTests()
        {
            // Setup DbContext
            DbContextOptions<ChatDbContext> options = new DbContextOptionsBuilder<ChatDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ChatDbContext(options);

            // Setup PasswordService
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json")
                .Build();

            IConfigurationSection encryptionSettings = configuration.GetSection("EncryptionSettings");
            string? key = encryptionSettings["Key"];
            string? iv = encryptionSettings["IV"];
            _passwordService = new PasswordService(key, iv);
        }

        [Fact]
        public async Task CreateChat_ValidModel_ReturnsCreatedAtActionResult()
        {
            // Arrange
            ChatsController controller = new(_dbContext, _passwordService);
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
            Chat chat = new()
            {
                Title = "Test Chat"
            };
            _dbContext.Chats.Add(chat);
            await _dbContext.SaveChangesAsync();

            ChatsController controller = new(_dbContext, _passwordService);

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
            ChatsController controller = new(_dbContext, _passwordService);

            // Act
            IActionResult result = await controller.GetChat("NonExistingChatId");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetAllChats_ReturnsOkResult()
        {
            // Arrange
            _dbContext.Chats.Add(new Chat { Title = "Test Chat" });
            await _dbContext.SaveChangesAsync();

            ChatsController controller = new(_dbContext, _passwordService);

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
            Chat chat = new() { Title = "Test Chat", MembersCount = 5 };
            _dbContext.Chats.Add(chat);
            await _dbContext.SaveChangesAsync();

            ChatsController controller = new(_dbContext, _passwordService);

            // Act
            IActionResult result = await controller.GetChatDetails(chat.ChatId);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            ChatDetailsViewModel details = Assert.IsType<ChatDetailsViewModel>(okResult.Value);
            Assert.Equal("Test Chat", details.Title);
            Assert.Equal(5, details.MembersCount);
        }

        [Fact]
        public async Task UpdateChat_ValidModel_ReturnsOkResult()
        {
            //Arrange
            Chat chat = new() { Title = "Test Chat" };
            _dbContext.Chats.Add(chat);
            await _dbContext.SaveChangesAsync();

            ChatsController controller = new(_dbContext, _passwordService);

            UpdateChatModel model = new()
            {
                Title = "New Title"
            };

            // Act
            IActionResult result = await controller.UpdateChat(chat.ChatId, model);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal("New Title", chat.Title);
        }

        [Fact]
        public async Task GetMessages_ExistingChatId_ReturnsOkObjectResult()
        {
            // Arrange
            Chat chat = new()
            {
                ChatId = "8c24a1d871f3452",
                Title = "Test Chat",
                Messages = new List<Message>
                {
                    new() { MessageId = "4bbdb5d8fb564d3", MessageText = _passwordService.EncryptString("Hello"), SenderId = "70b733415e1d4e7", ChatId = "8c24a1d871f3452", CreatedAt = DateTime.UtcNow },
                    new() { MessageId = "9cd13253f94f466", MessageText = _passwordService.EncryptString("How are you?"), SenderId = "70b733415e1d4e7", ChatId = "8c24a1d871f3452", CreatedAt = DateTime.UtcNow }
                }
            };
            _dbContext.Chats.Add(chat);
            await _dbContext.SaveChangesAsync();

            ChatsController controller = new(_dbContext, _passwordService);

            // Act
            IActionResult result = await controller.GetMessages(chat.ChatId);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            List<MessageViewModel> returnedMessages = Assert.IsAssignableFrom<IEnumerable<MessageViewModel>>(okResult.Value).ToList();
            Assert.Equal(2, returnedMessages.Count);

            // Verify decrypted message text
            Assert.Equal("Hello", returnedMessages[0].MessageText);
            Assert.Equal("How are you?", returnedMessages[1].MessageText);
        }

        [Fact]
        public async Task GetMessages_NonExistingChatId_ReturnsNotFoundResult()
        {
            // Arrange
            ChatsController controller = new(_dbContext, _passwordService);

            // Act
            IActionResult result = await controller.GetMessages("NonExistingChatId");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ClearMessages_ExistingChatId_ReturnsOkResult()
        {
            // Arrange
            Chat chat = new() { ChatId = "8c24a1d871f3452", Title = "Test Chat" };
            Message message1 = new() { ChatId = chat.ChatId, MessageText = _passwordService.EncryptString("Hello"), CreatedAt = DateTime.UtcNow };
            Message message2 = new() { ChatId = chat.ChatId, MessageText = _passwordService.EncryptString("How are you?"), CreatedAt = DateTime.UtcNow };

            _dbContext.Chats.Add(chat);
            _dbContext.Messages.AddRange(message1, message2);
            await _dbContext.SaveChangesAsync();

            ChatsController controller = new(_dbContext, _passwordService);

            // Act
            IActionResult result = await controller.ClearMessages(chat.ChatId);

            // Assert
            Assert.IsType<OkObjectResult>(result);

            // Verify messages are cleared
            Chat? clearedChat = await _dbContext.Chats
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.ChatId == chat.ChatId);

            Assert.NotNull(clearedChat);
            Assert.Empty(clearedChat.Messages);
        }

        [Fact]
        public async Task ClearMessages_NonExistingChatId_ReturnsNotFoundResult()
        {
            // Arrange
            ChatsController controller = new ChatsController(_dbContext, _passwordService);

            // Act
            IActionResult result = await controller.ClearMessages("NonExistingChatId");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
