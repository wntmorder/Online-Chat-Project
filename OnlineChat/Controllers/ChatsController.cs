using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineChat.Data;
using OnlineChat.Models;
using OnlineChat.ViewModels;

namespace OnlineChat.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatsController : ControllerBase
    {
        private readonly ChatDbContext _dbContext;

        public ChatsController(ChatDbContext dbContext)
        {
            _dbContext = dbContext;
        }

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

        [HttpGet]
        public async Task<IActionResult> GetAllChats()
        {
            List<Chat> chats = await _dbContext.Chats.ToListAsync();
            return Ok(chats);
        }

        // Get chat details (title and members count)
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
    }
}
