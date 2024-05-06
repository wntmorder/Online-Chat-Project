using System.ComponentModel.DataAnnotations;

namespace OnlineChat.Models
{
    public class Message
    {
        public string? MessageId { get; set; } = Guid.NewGuid().ToString("N")[..15];

        [Required] public string? MessageText { get; set; }

        public string? SenderId { get; set; }

        public User? Sender { get; set; }

        public string? ChatId { get; set; }

        public Chat? Chat { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}