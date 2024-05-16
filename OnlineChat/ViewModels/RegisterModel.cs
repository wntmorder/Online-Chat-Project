using System.ComponentModel.DataAnnotations;

namespace OnlineChat.ViewModels
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Email not specified")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Username not specified")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password not specified")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password entered incorrectly")]
        public string? ConfirmPassword { get; set; }
    }
}
