using System.ComponentModel.DataAnnotations;

namespace PortalBeritaApp.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Nama tampilan wajib diisi")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Nama tampilan minimal 2 dan maksimal 50 karakter")]
        [Display(Name = "Nama Lengkap / Tampilan")]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email wajib diisi")]
        [EmailAddress(ErrorMessage = "Format email tidak valid")]
        [Display(Name = "Alamat Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password wajib diisi")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password minimal 8 karakter")]
        [DataType(DataType.Password)]
        [Display(Name = "Kata Sandi")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Konfirmasi Kata Sandi")]
        [Compare("Password", ErrorMessage = "Konfirmasi kata sandi tidak cocok dengan kata sandi")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
