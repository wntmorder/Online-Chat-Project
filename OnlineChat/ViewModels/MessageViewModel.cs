namespace OnlineChat.ViewModels
{
    public class MessageViewModel
    {
        public string? MessageId { get; set; }
        public string? MessageText { get; set; }
        public string? SenderId { get; set; }
        public string? SenderUsername { get; set; }
        public string? ChatId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
