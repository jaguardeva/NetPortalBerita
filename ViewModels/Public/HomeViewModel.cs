namespace PortalBeritaApp.ViewModels.Public
{
    public class ArticleCardViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string? FeaturedImageUrl { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategorySlug { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public int ReadingTimeMinutes { get; set; } = 1;
    }

    public class CategoryViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int ArticleCount { get; set; }
    }

    public class CategorySectionViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public string CategorySlug { get; set; } = string.Empty;
        public List<ArticleCardViewModel> Articles { get; set; } = new();
    }

    public class HomeViewModel
    {
        public ArticleCardViewModel? BreakingNews { get; set; }
        public ArticleCardViewModel? HeroArticle { get; set; }
        public List<ArticleCardViewModel> SecondaryHeroArticles { get; set; } = new();
        public List<ArticleCardViewModel> LatestArticles { get; set; } = new();
        public List<ArticleCardViewModel> PopularArticles { get; set; } = new();
        public List<CategorySectionViewModel> CategorySections { get; set; } = new();
        public List<CategoryViewModel> Categories { get; set; } = new();
    }
}
