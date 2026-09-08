using System.ComponentModel.DataAnnotations;

namespace PortalBeritaApp.Models
{
    /// <summary>
    /// Category entity for grouping articles.
    /// Per PRD §24: Name and Slug must be unique.
    /// </summary>
    public class Category
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Slug { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Article> Articles { get; set; } = new List<Article>();
    }
}
