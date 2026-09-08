using PortalBeritaApp.Models.Enums;

namespace PortalBeritaApp.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalArticles { get; set; }
        public int PublishedArticles { get; set; }
        public int DraftArticles { get; set; }

        public int TotalCategories { get; set; }

        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int AuthorsCount { get; set; }

        public int TotalComments { get; set; }
        public int TotalLikes { get; set; }

        public List<AdminRecentArticleItem> RecentArticles { get; set; } = new();
        public List<AdminRecentCommentItem> RecentComments { get; set; } = new();
    }

    public class AdminRecentArticleItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public ArticleStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminRecentCommentItem
    {
        public Guid Id { get; set; }
        public string ArticleTitle { get; set; } = string.Empty;
        public string ArticleSlug { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public CommentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
