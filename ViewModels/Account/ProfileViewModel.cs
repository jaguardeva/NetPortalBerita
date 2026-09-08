using System.ComponentModel.DataAnnotations;

namespace PortalBeritaApp.ViewModels.Account
{
    public class ProfileViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Alamat Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama tampilan wajib diisi")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Nama tampilan minimal 2 dan maksimal 50 karakter")]
        [Display(Name = "Nama Lengkap / Tampilan")]
        public string DisplayName { get; set; } = string.Empty;

        public IList<string> Roles { get; set; } = new List<string>();

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; }

        public int ArticlesCount { get; set; }
        public int CommentsCount { get; set; }
        public int LikesCount { get; set; }

        public ChangePasswordViewModel? ChangePassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Kata sandi saat ini wajib diisi")]
        [DataType(DataType.Password)]
        [Display(Name = "Kata Sandi Saat Ini")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kata sandi baru wajib diisi")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Kata sandi baru minimal 8 karakter")]
        [DataType(DataType.Password)]
        [Display(Name = "Kata Sandi Baru")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Konfirmasi Kata Sandi Baru")]
        [Compare("NewPassword", ErrorMessage = "Konfirmasi kata sandi baru tidak cocok")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
