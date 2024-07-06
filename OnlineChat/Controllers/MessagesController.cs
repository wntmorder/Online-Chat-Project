using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineChat.Data;
using OnlineChat.Models;
using OnlineChat.Services;
using OnlineChat.ViewModels;

namespace OnlineChat.Controllers
{
    /// <summary>
    /// Controller for managing messages in chats.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly ChatDbContext _dbContext;
        private readonly PasswordService _passwordService;

        public MessagesController(ChatDbContext dbContext, PasswordService passwordService)
        {
            _dbContext = dbContext;
            _passwordService = passwordService;
        }

        /// <summary>
        /// Creates a new message.
        /// </summary>
        /// <param name="model">The model containing message details.</param>
        /// <returns>ActionResult with the created message.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateMessage(CreateMessageModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string encryptedMessageText = _passwordService.EncryptString(model.MessageText);

            Message message = new()
            {
                MessageText = encryptedMessageText,
                SenderId = model.SenderId,
                ChatId = model.ChatId,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMessage), new { messageId = message.MessageId }, message);
        }

        /// <summary>
        /// Gets a message by its ID.
        /// </summary>
        /// <param name="messageId">The ID of the message to retrieve.</param>
        /// <returns>ActionResult with the message details.</returns>
        [HttpGet("{messageId}")]
        public async Task<IActionResult> GetMessage(string messageId)
        {
            Message? message = await _dbContext.Messages
                .Include(m => m.Sender)
                .FirstOrDefaultAsync(m => m.MessageId == messageId);

            if (message == null)
            {
                return NotFound();
            }

            string decryptedMessageText = _passwordService.DecryptString(message.MessageText);

            var result = new
            {
                message.MessageId,
                MessageText = decryptedMessageText,
                message.SenderId,
                SenderUsername = message.Sender?.Username,
                message.ChatId,
                message.CreatedAt
            };

            return Ok(result);
        }

        /// <summary>
        /// Updates a message in a chat.
        /// </summary>
        /// <param name="messageId">The ID of the message to update.</param>
        /// <param name="model">The model containing updated message details.</param>
        /// <returns>ActionResult with status of the operation.</returns>
        [HttpPut("{messageId}")]
        public async Task<IActionResult> UpdateMessage(string messageId, UpdateMessageModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Message? message = await _dbContext.Messages.FirstOrDefaultAsync(m => m.MessageId == messageId);
            if (message == null)
            {
                return NotFound("Message not found");
            }

            message.MessageText = _passwordService.EncryptString(model.MessageText);

            await _dbContext.SaveChangesAsync();
            return Ok("Message updated successfully");
        }

        /// <summary>
        /// Deletes a message by its ID.
        /// </summary>
        /// <param name="messageId">The ID of the message to delete.</param>
        /// <returns>ActionResult with status of the operation.</returns>
        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessage(string messageId)
        {
            Message? message = await _dbContext.Messages.FindAsync(messageId);
            if (message == null)
            {
                return NotFound();
            }

            _dbContext.Messages.Remove(message);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
