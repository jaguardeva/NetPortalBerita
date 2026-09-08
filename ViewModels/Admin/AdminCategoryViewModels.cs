using System.ComponentModel.DataAnnotations;

namespace PortalBeritaApp.ViewModels.Admin
{
    public class AdminCategoryItemViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ArticleCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CategoryCreateEditViewModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Nama kategori wajib diisi")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Nama kategori minimal 2 dan maksimal 100 karakter")]
        [Display(Name = "Nama Kategori")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Deskripsi maksimal 500 karakter")]
        [Display(Name = "Deskripsi Singkat (Opsional)")]
        public string? Description { get; set; }
    }
}
