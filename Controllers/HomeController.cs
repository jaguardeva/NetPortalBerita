using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalBeritaApp.Data;
using PortalBeritaApp.Models;
using PortalBeritaApp.Models.Enums;
using PortalBeritaApp.ViewModels.Public;

namespace PortalBeritaApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Base query: only published articles with relations
            var baseQuery = _context.Articles
                .Include(a => a.Category)
                .Include(a => a.Author)
                .Include(a => a.Likes)
                .Include(a => a.Comments)
                .Where(a => a.Status == ArticleStatus.Published && a.PublishedAt != null);

            // 1. Hero & Secondary Articles
            var topArticles = await baseQuery
                .OrderByDescending(a => a.PublishedAt)
                .Take(4)
                .Select(a => MapToCard(a))
                .ToListAsync();

            var heroArticle = topArticles.FirstOrDefault();
            var secondaryHero = topArticles.Skip(1).Take(3).ToList();

            // Breaking news: the most recent published article
            var breakingNews = heroArticle;

            // 2. Latest Articles (skip hero top 4, take 6)
            var latestArticles = await baseQuery
                .OrderByDescending(a => a.PublishedAt)
                .Skip(4)
                .Take(6)
                .Select(a => MapToCard(a))
                .ToListAsync();

            // 3. Popular Articles (ordered by Likes count, then PublishedAt)
            var popularArticles = await baseQuery
                .OrderByDescending(a => a.Likes.Count)
                .ThenByDescending(a => a.PublishedAt)
                .Take(5)
                .Select(a => MapToCard(a))
                .ToListAsync();

            // 4. Categories with published article counts
            var categories = await _context.Categories
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    ArticleCount = c.Articles.Count(a => a.Status == ArticleStatus.Published)
                })
                .OrderByDescending(c => c.ArticleCount)
                .ToListAsync();

            // 5. Category Sections: Top 3 categories with published articles
            var categorySections = new List<CategorySectionViewModel>();
            var topCategories = categories.Where(c => c.ArticleCount > 0).Take(3).ToList();

            foreach (var cat in topCategories)
            {
                var catArticles = await baseQuery
                    .Where(a => a.CategoryId == cat.Id)
                    .OrderByDescending(a => a.PublishedAt)
                    .Take(4)
                    .Select(a => MapToCard(a))
                    .ToListAsync();

                if (catArticles.Any())
                {
                    categorySections.Add(new CategorySectionViewModel
                    {
                        CategoryName = cat.Name,
                        CategorySlug = cat.Slug,
                        Articles = catArticles
                    });
                }
            }

            var viewModel = new HomeViewModel
            {
                BreakingNews = breakingNews,
                HeroArticle = heroArticle,
                SecondaryHeroArticles = secondaryHero,
                LatestArticles = latestArticles,
                PopularArticles = popularArticles,
                CategorySections = categorySections,
                Categories = categories
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? id)
        {
            var code = id ?? Response.StatusCode;
            string message;

            if (code == 404)
            {
                message = "Halaman atau artikel yang Anda cari tidak ditemukan atau telah dipindahkan.";
            }
            else if (code == 403)
            {
                message = "Anda tidak memiliki izin yang diperlukan untuk mengakses halaman ini.";
            }
            else
            {
                message = "Terjadi kendala pada sistem kami. Tim kami sedang menindaklanjuti hal ini.";
            }

            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = code,
                Message = message
            });
        }

        private static ArticleCardViewModel MapToCard(Article a)
        {
            int words = string.IsNullOrWhiteSpace(a.Content) ? 0 : a.Content.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
            int readingTime = Math.Max(1, (int)Math.Ceiling(words / 200.0));

            return new ArticleCardViewModel
            {
                Id = a.Id,
                Title = a.Title,
                Slug = a.Slug,
                Excerpt = a.Excerpt ?? string.Empty,
                FeaturedImageUrl = a.FeaturedImageUrl,
                CategoryName = a.Category != null ? a.Category.Name : "Umum",
                CategorySlug = a.Category != null ? a.Category.Slug : "umum",
                AuthorName = a.Author != null ? a.Author.DisplayName : "Redaksi",
                PublishedAt = a.PublishedAt ?? a.CreatedAt,
                LikesCount = a.Likes.Count,
                CommentsCount = a.Comments.Count,
                ReadingTimeMinutes = readingTime
            };
        }
    }
}
