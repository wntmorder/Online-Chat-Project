using System.ComponentModel.DataAnnotations;

namespace OnlineChat.ViewModels
{
    public class CreateMessageModel
    {
        [Required(ErrorMessage = "Message text is required")]
        public string? MessageText { get; set; }

        [Required(ErrorMessage = "Sender ID is required")]
        public string? SenderId { get; set; }

        [Required(ErrorMessage = "Chat ID is required")]
        public string? ChatId { get; set; }
    }
}
