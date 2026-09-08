using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalBeritaApp.Data;
using PortalBeritaApp.Models;
using PortalBeritaApp.Models.Enums;
using PortalBeritaApp.ViewModels.Public;

namespace PortalBeritaApp.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /search?q={query}&page={page}
        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> Index(string? q, int page = 1)
        {
            int pageSize = 9;
            if (page < 1) page = 1;

            var trimmedQuery = q?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(trimmedQuery))
            {
                return View(new SearchResultsViewModel
                {
                    Query = string.Empty,
                    Articles = new List<ArticleCardViewModel>(),
                    CurrentPage = 1,
                    TotalPages = 0,
                    TotalCount = 0,
                    PageSize = pageSize
                });
            }

            var lowerQ = trimmedQuery.ToLower();

            var query = _context.Articles
                .Include(a => a.Category)
                .Include(a => a.Author)
                .Include(a => a.Likes)
                .Include(a => a.Comments)
                .Where(a => a.Status == ArticleStatus.Published && a.PublishedAt != null)
                .Where(a => a.Title.ToLower().Contains(lowerQ)
                         || (a.Excerpt != null && a.Excerpt.ToLower().Contains(lowerQ))
                         || a.Content.ToLower().Contains(lowerQ));

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var articles = await query
                .OrderByDescending(a => a.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => MapToCard(a))
                .ToListAsync();

            var viewModel = new SearchResultsViewModel
            {
                Query = trimmedQuery,
                Articles = articles,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };

            return View(viewModel);
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
