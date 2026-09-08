using PortalBeritaApp.Models.Enums;

namespace PortalBeritaApp.ViewModels.Author
{
    public class ArticleListViewModel
    {
        public List<ArticleItemViewModel> Articles { get; set; } = new();

        public ArticleStatus? StatusFilter { get; set; }
        public string? SearchQuery { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 10;

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    public class ArticleItemViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? FeaturedImageUrl { get; set; }
        public ArticleStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
    }
}
