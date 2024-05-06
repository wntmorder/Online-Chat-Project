using System.ComponentModel.DataAnnotations;

namespace OnlineChat.Models
{
    public class Chat
    {
        public string? ChatId { get; set; } = Guid.NewGuid().ToString("N")[..15];

        [Required] public string? Title { get; set; }

        public int? MembersCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Message>? Messages { get; set; }
    }
}
