using Microsoft.EntityFrameworkCore;
using OnlineChat.Models;

namespace OnlineChat.Data
{
    public class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options)
    {
        public DbSet<User>? Users { get; set; }
        public DbSet<Chat>? Chats { get; set; }
        public DbSet<Message>? Messages { get; set; }
    }
}
