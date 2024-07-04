using System.ComponentModel.DataAnnotations;

namespace OnlineChat.ViewModels
{
    public class CreateChatModel
    {
        [Required(ErrorMessage = "Title is required")]
        public string? Title { get; set; }
    }
}
