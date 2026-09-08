using System.ComponentModel.DataAnnotations;
using PortalBeritaApp.Models.Enums;

namespace PortalBeritaApp.Models
{
    /// <summary>
    /// Comment entity for article discussions.
    /// Per PRD §26-28: Content required, max 2000 chars, has CommentStatus for moderation.
    /// </summary>
    public class Comment
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        public CommentStatus Status { get; set; } = CommentStatus.Published;

        public Guid ArticleId { get; set; }

        public Guid UserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Article Article { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}
