using System.ComponentModel.DataAnnotations;

namespace OnlineChat.ViewModels
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Email or username not specified")]
        public string? EmailOrUsername { get; set; }

        [Required(ErrorMessage = "Password not specified")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
