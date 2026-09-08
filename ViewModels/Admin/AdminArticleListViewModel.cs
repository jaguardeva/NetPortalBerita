using Microsoft.AspNetCore.Mvc.Rendering;
using PortalBeritaApp.Models.Enums;

namespace PortalBeritaApp.ViewModels.Admin
{
    public class AdminArticleListViewModel
    {
        public List<AdminArticleItemViewModel> Articles { get; set; } = new();

        public Guid? CategoryFilter { get; set; }
        public ArticleStatus? StatusFilter { get; set; }
        public string? SearchQuery { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 15;

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();
    }

    public class AdminArticleItemViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public ArticleStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
    }
}
