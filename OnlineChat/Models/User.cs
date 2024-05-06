using System.ComponentModel.DataAnnotations;

namespace OnlineChat.Models
{
    public class User
    {
        public string? UserId { get; private set; } = Guid.NewGuid().ToString("N")[..15];

        [Required] public string? Username { get; set; }

        [Required] public string? Email { get; set; }

        [Required] public string? PasswordHash { get; set; }

        [Required] public string? Salt { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
