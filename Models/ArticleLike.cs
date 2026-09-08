namespace PortalBeritaApp.Models
{
    /// <summary>
    /// Junction entity for article likes.
    /// Per PRD §29-30: Unique composite index on (ArticleId, UserId) enforced at DB level.
    /// </summary>
    public class ArticleLike
    {
        public Guid Id { get; set; }

        public Guid ArticleId { get; set; }

        public Guid UserId { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Article Article { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}
