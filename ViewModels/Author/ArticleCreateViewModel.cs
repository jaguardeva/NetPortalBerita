using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using PortalBeritaApp.Models.Enums;

namespace PortalBeritaApp.ViewModels.Author
{
    public class ArticleCreateViewModel
    {
        [Required(ErrorMessage = "Judul artikel wajib diisi")]
        [StringLength(200, MinimumLength = 10, ErrorMessage = "Judul minimal 10 dan maksimal 200 karakter")]
        [Display(Name = "Judul Artikel")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori wajib dipilih")]
        [Display(Name = "Kategori")]
        public Guid? CategoryId { get; set; }

        [Required(ErrorMessage = "Ringkasan (excerpt) wajib diisi")]
        [StringLength(300, MinimumLength = 20, ErrorMessage = "Ringkasan minimal 20 dan maksimal 300 karakter")]
        [Display(Name = "Ringkasan Singkat")]
        public string Excerpt { get; set; } = string.Empty;

        [Required(ErrorMessage = "Konten artikel wajib diisi")]
        [MinLength(50, ErrorMessage = "Konten artikel minimal 50 karakter")]
        [Display(Name = "Isi Konten Artikel")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Gambar Utama / Sampul")]
        public IFormFile? FeaturedImage { get; set; }

        [Display(Name = "Status Publikasi")]
        public ArticleStatus Status { get; set; } = ArticleStatus.Draft;

        public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
