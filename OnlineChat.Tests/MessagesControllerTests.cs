using Microsoft.Extensions.Configuration;

namespace OnlineChat.Tests
{
    public class MessagesControllerTests : IDisposable
    {
        private readonly ChatDbContext _dbContext;
        private readonly PasswordService _passwordService;

        public MessagesControllerTests()
        {
            DbContextOptions<ChatDbContext> options = new DbContextOptionsBuilder<ChatDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ChatDbContext(options);

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json")
                .Build();

            IConfigurationSection encryptionSettings = configuration.GetSection("EncryptionSettings");
            string? key = encryptionSettings["Key"];
            string? iv = encryptionSettings["IV"];

            _passwordService = new PasswordService(key, iv);
        }

        [Fact]
        public async Task CreateMessage_ValidModel_ReturnsCreatedAtActionResult()
        {
            MessagesController controller = new(_dbContext, _passwordService);

            CreateMessageModel model = new()
            {
                MessageText = "Hello, world!",
                SenderId = "70b733415e1d4e7",
                ChatId = "8c24a1d871f3452"
            };

            IActionResult result = await controller.CreateMessage(model);

            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task GetMessage_ValidId_ReturnsOkResult()
        {
            MessagesController controller = new(_dbContext, _passwordService);

            Message message = new()
            {
                MessageId = "9cd13253f94f466",
                MessageText = _passwordService.EncryptString("Hello, world!"),
                SenderId = "70b733415e1d4e7",
                ChatId = "8c24a1d871f3452",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            IActionResult result = await controller.GetMessage("9cd13253f94f466");

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateMessage_ValidId_ReturnsOkResult()
        {
            MessagesController controller = new(_dbContext, _passwordService);

            Message message = new()
            {
                MessageId = "9cd13253f94f466",
                MessageText = _passwordService.EncryptString("Old text"),
                SenderId = "70b733415e1d4e7",
                ChatId = "8c24a1d871f3452",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            UpdateMessageModel model = new()
            {
                MessageText = "New text"
            };

            IActionResult result = await controller.UpdateMessage("9cd13253f94f466", model);

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal("New text", _passwordService.DecryptString(message.MessageText));
        }


        [Fact]
        public async Task DeleteMessage_ValidId_ReturnsNoContent()
        {
            MessagesController controller = new(_dbContext, _passwordService);

            Message message = new()
            {
                MessageId = "9cd13253f94f466",
                MessageText = _passwordService.EncryptString("Hello, world!"),
                SenderId = "70b733415e1d4e7",
                ChatId = "8c24a1d871f3452",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            IActionResult result = await controller.DeleteMessage("9cd13253f94f466");

            Assert.IsType<NoContentResult>(result);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}
