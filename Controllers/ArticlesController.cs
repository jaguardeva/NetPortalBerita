using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalBeritaApp.Data;
using PortalBeritaApp.Models;
using PortalBeritaApp.Models.Enums;
using PortalBeritaApp.ViewModels.Public;

namespace PortalBeritaApp.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ArticlesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /articles or /Articles
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 12;
            if (page < 1) page = 1;

            var query = _context.Articles
                .Include(a => a.Category)
                .Include(a => a.Author)
                .Include(a => a.Likes)
                .Include(a => a.Comments)
                .Where(a => a.Status == ArticleStatus.Published && a.PublishedAt != null);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var articles = await query
                .OrderByDescending(a => a.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => MapToCard(a))
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;

            return View(articles);
        }

        // GET: /article/{slug}
        [HttpGet]
        [Route("article/{slug}")]
        public async Task<IActionResult> Detail(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return NotFound();
            }

            var article = await _context.Articles
                .Include(a => a.Category)
                .Include(a => a.Author)
                .Include(a => a.Likes)
                .Include(a => a.Comments.Where(c => c.Status == CommentStatus.Published))
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(a => a.Slug == slug && a.Status == ArticleStatus.Published);

            if (article == null)
            {
                return NotFound();
            }

            // Current user context
            Guid? currentUserId = null;
            bool isUserAdmin = User.IsInRole("Admin");

            if (User.Identity?.IsAuthenticated == true)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    currentUserId = currentUser.Id;
                }
            }

            bool isLiked = false;
            if (currentUserId.HasValue)
            {
                isLiked = article.Likes.Any(l => l.UserId == currentUserId.Value);
            }

            // Related articles (same category, different id)
            var relatedArticles = await _context.Articles
                .Include(a => a.Category)
                .Include(a => a.Author)
                .Include(a => a.Likes)
                .Include(a => a.Comments)
                .Where(a => a.CategoryId == article.CategoryId
                         && a.Id != article.Id
                         && a.Status == ArticleStatus.Published
                         && a.PublishedAt != null)
                .OrderByDescending(a => a.PublishedAt)
                .Take(3)
                .Select(a => MapToCard(a))
                .ToListAsync();

            // Map comments
            var comments = article.Comments
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentItemViewModel
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    UserDisplayName = c.User != null ? c.User.DisplayName : "Pengguna",
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    CanDelete = currentUserId.HasValue && (c.UserId == currentUserId.Value || isUserAdmin)
                })
                .ToList();

            int words = string.IsNullOrWhiteSpace(article.Content) ? 0 : article.Content.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
            int readingTime = Math.Max(1, (int)Math.Ceiling(words / 200.0));

            var viewModel = new ArticleDetailViewModel
            {
                Id = article.Id,
                Title = article.Title,
                Slug = article.Slug,
                Excerpt = article.Excerpt ?? string.Empty,
                Content = article.Content,
                FeaturedImageUrl = article.FeaturedImageUrl,
                CategoryId = article.CategoryId,
                CategoryName = article.Category != null ? article.Category.Name : "Umum",
                CategorySlug = article.Category != null ? article.Category.Slug : "umum",
                AuthorId = article.AuthorId,
                AuthorName = article.Author != null ? article.Author.DisplayName : "Redaksi",
                PublishedAt = article.PublishedAt ?? article.CreatedAt,
                UpdatedAt = article.UpdatedAt,
                LikesCount = article.Likes.Count,
                CommentsCount = comments.Count,
                IsLikedByCurrentUser = isLiked,
                ReadingTimeMinutes = readingTime,
                RelatedArticles = relatedArticles,
                Comments = comments
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
