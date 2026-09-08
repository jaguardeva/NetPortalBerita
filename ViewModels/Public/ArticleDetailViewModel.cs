namespace PortalBeritaApp.ViewModels.Public
{
    public class ArticleDetailViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? FeaturedImageUrl { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategorySlug { get; set; } = string.Empty;
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public int ReadingTimeMinutes { get; set; } = 1;

        public List<ArticleCardViewModel> RelatedArticles { get; set; } = new();
        public List<CommentItemViewModel> Comments { get; set; } = new();
    }

    public class CommentItemViewModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserDisplayName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool CanDelete { get; set; }
    }
}
