using Microsoft.AspNetCore.Identity;

namespace PortalBeritaApp.Models
{
    /// <summary>
    /// Custom Identity user extending IdentityUser with Guid PK.
    /// Per PRD §7: ApplicationUser : IdentityUser<Guid>
    /// </summary>
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string DisplayName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<Article> Articles { get; set; } = new List<Article>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<ArticleLike> ArticleLikes { get; set; } = new List<ArticleLike>();
    }
}
