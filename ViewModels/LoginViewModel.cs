using System.ComponentModel.DataAnnotations;

namespace Jahongir_diplomIshi.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email kiritilmadi")]
        [EmailAddress(ErrorMessage = "Noto'g'ri email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Parol kiritilmadi")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
