using System.ComponentModel.DataAnnotations;

namespace PortalBeritaApp.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email wajib diisi")]
        [EmailAddress(ErrorMessage = "Format email tidak valid")]
        [Display(Name = "Alamat Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password wajib diisi")]
        [DataType(DataType.Password)]
        [Display(Name = "Kata Sandi")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Ingat saya di perangkat ini")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
