using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineChat.Data;
using OnlineChat.Models;
using OnlineChat.Services;
using OnlineChat.ViewModels;

namespace OnlineChat.Controllers
{
    /// <summary>
    /// Controller for managing chats and their messages.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ChatsController : ControllerBase
    {
        private readonly ChatDbContext _dbContext;
        private readonly EncryptionService _encryptionService;

        public ChatsController(ChatDbContext dbContext, EncryptionService encryptionService)
        {
            _dbContext = dbContext;
            _encryptionService = encryptionService;
        }

        /// <summary>
        /// Creates a new chat.
        /// </summary>
        /// <param name="model">The model containing chat details.</param>
        /// <returns>ActionResult with the created chat.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateChat(CreateChatModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Chat chat = new()
            {
                Title = model.Title,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Chats.Add(chat);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetChat), new { chatId = chat.ChatId }, chat);
        }

        /// <summary>
        /// Gets a chat by its ID.
        /// </summary>
        /// <param name="chatId">The ID of the chat to retrieve.</param>
        /// <returns>ActionResult with the chat details.</returns>
        [HttpGet("{chatId}")]
        public async Task<IActionResult> GetChat(string chatId)
        {
            Chat? chat = await _dbContext.Chats
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.ChatId == chatId);

            if (chat == null)
            {
                return NotFound();
            }

            return Ok(chat);
        }

        /// <summary>
        /// Gets all chats.
        /// </summary>
        /// <returns>ActionResult with a list of all chats.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllChats()
        {
            List<Chat> chats = await _dbContext.Chats.ToListAsync();
            return Ok(chats);
        }

        /// <summary>
        /// Gets chat details such as title and members count.
        /// </summary>
        /// <param name="chatId">The ID of the chat to retrieve details for.</param>
        /// <returns>ActionResult with chat details.</returns>
        [HttpGet("{chatId}/details")]
        public async Task<IActionResult> GetChatDetails(string chatId)
        {
            Chat? chat = await _dbContext.Chats.FirstOrDefaultAsync(c => c.ChatId == chatId);

            if (chat == null)
            {
                return NotFound();
            }

            ChatDetailsViewModel details = new()
            {
                Title = chat.Title,
                MembersCount = chat.MembersCount
            };

            return Ok(details);
        }

        /// <summary>
        /// Updates the chat details.
        /// </summary>
        /// <param name="chatId">The ID of the chat to update.</param>
        /// <param name="model">The model containing updated chat details.</param>
        /// <returns>ActionResult with status of the operation.</returns>
        [HttpPut("{chatId}")]
        public async Task<IActionResult> UpdateChat(string chatId, UpdateChatModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Chat? chat = await _dbContext.Chats.FindAsync(chatId);
            if (chat == null)
            {
                return NotFound("Chat not found");
            }

            chat.Title = model.Title ?? chat.Title;

            await _dbContext.SaveChangesAsync();
            return Ok("Chat updated successfully");
        }


        /// <summary>
        /// Gets all messages in a chat.
        /// </summary>
        /// <param name="chatId">The ID of the chat to retrieve messages for.</param>
        /// <returns>ActionResult with a list of messages.</returns>
        [HttpGet("{chatId}/messages")]
        public async Task<IActionResult> GetMessages(string chatId)
        {
            Chat? chat = await _dbContext.Chats
                .Include(c => c.Messages)
                .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(c => c.ChatId == chatId);

            if (chat == null)
            {
                return NotFound();
            }

            List<MessageViewModel> messages = new();

            foreach (Message message in chat.Messages)
            {
                messages.Add(new MessageViewModel
                {
                    MessageId = message.MessageId,
                    MessageText = _encryptionService.DecryptString(message.MessageText),
                    SenderId = message.SenderId,
                    SenderUsername = message.Sender?.Username,
                    ChatId = message.ChatId,
                    CreatedAt = message.CreatedAt
                });
            }

            return Ok(messages);
        }

        /// <summary>
        /// Clears all messages in a chat.
        /// </summary>
        /// <param name="chatId">The ID of the chat to clear messages from.</param>
        /// <returns>ActionResult with status of the operation.</returns>
        [HttpDelete("{chatId}/messages")]
        public async Task<IActionResult> ClearMessages(string chatId)
        {
            Chat? chat = await _dbContext.Chats
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.ChatId == chatId);

            if (chat == null)
            {
                return NotFound("Chat not found");
            }

            _dbContext.Messages.RemoveRange(chat.Messages);
            await _dbContext.SaveChangesAsync();

            return Ok("Messages cleared successfully");
        }
    }
}
