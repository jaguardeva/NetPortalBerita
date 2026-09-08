namespace PortalBeritaApp.ViewModels.Public
{
    public class CategoryDetailViewModel
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<ArticleCardViewModel> Articles { get; set; } = new();

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 9;

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    public class SearchResultsViewModel
    {
        public string Query { get; set; } = string.Empty;
        public List<ArticleCardViewModel> Articles { get; set; } = new();

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 9;

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
