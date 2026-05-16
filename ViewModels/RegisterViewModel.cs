using System.ComponentModel.DataAnnotations;

namespace Jahongir_diplomIshi.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ism kiritilmadi")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email kiritilmadi")]
        [EmailAddress(ErrorMessage = "Noto'g'ri email format")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Parol kiritilmadi")]
        [MinLength(6, ErrorMessage = "Parol kamida 6 ta belgi bo'lishi kerak")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Parolni tasdiqlang")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Parollar mos kelmadi")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
