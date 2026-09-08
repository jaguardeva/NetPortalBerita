using System.ComponentModel.DataAnnotations;
using PortalBeritaApp.Models.Enums;

namespace PortalBeritaApp.Models
{
    /// <summary>
    /// Article entity — the core content of the platform.
    /// Per PRD §12: Fields include Title, Slug (unique), Excerpt, Content, FeaturedImageUrl, Status, AuthorId, CategoryId.
    /// </summary>
    public class Article
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 10)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Slug { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Excerpt { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(500)]
        public string? FeaturedImageUrl { get; set; }

        public ArticleStatus Status { get; set; } = ArticleStatus.Draft;

        public Guid AuthorId { get; set; }

        public Guid CategoryId { get; set; }

        public DateTime? PublishedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ApplicationUser Author { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<ArticleLike> Likes { get; set; } = new List<ArticleLike>();
    }
}
