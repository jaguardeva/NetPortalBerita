using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PortalBeritaApp.Data;
using PortalBeritaApp.Models.Enums;
using PortalBeritaApp.Services;
using PortalBeritaApp.ViewModels.Admin;

namespace PortalBeritaApp.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Articles")]
    public class AdminArticlesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<AdminArticlesController> _logger;

        public AdminArticlesController(
            ApplicationDbContext context,
            IFileService fileService,
            ILogger<AdminArticlesController> logger)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
        }

        // GET: /Admin/Articles
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(int page = 1, Guid? categoryId = null, ArticleStatus? status = null, string? search = null)
        {
            int pageSize = 15;
            if (page < 1) page = 1;

            var query = _context.Articles
                .Include(a => a.Category)
                .Include(a => a.Author)
                .Include(a => a.Likes)
                .Include(a => a.Comments)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(a => a.CategoryId == categoryId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.Trim().ToLower();
                query = query.Where(a => a.Title.ToLower().Contains(lower)
                                      || (a.Author != null && a.Author.DisplayName.ToLower().Contains(lower)));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var articles = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AdminArticleItemViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Slug = a.Slug,
                    AuthorName = a.Author != null ? a.Author.DisplayName : "Anonim",
                    CategoryName = a.Category != null ? a.Category.Name : "Umum",
                    Status = a.Status,
                    CreatedAt = a.CreatedAt,
                    PublishedAt = a.PublishedAt,
                    LikesCount = a.Likes.Count,
                    CommentsCount = a.Comments.Count
                })
                .ToListAsync();

            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = categoryId.HasValue && c.Id == categoryId.Value
                })
                .ToListAsync();

            var viewModel = new AdminArticleListViewModel
            {
                Articles = articles,
                CategoryFilter = categoryId,
                StatusFilter = status,
                SearchQuery = search,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize,
                Categories = categories
            };

            return View(viewModel);
        }

        // POST: /Admin/Articles/ToggleStatus/{id}
        [HttpPost("ToggleStatus/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null) return NotFound();

            if (article.Status == ArticleStatus.Draft)
            {
                article.Status = ArticleStatus.Published;
                if (article.PublishedAt == null)
                {
                    article.PublishedAt = DateTime.UtcNow;
                }
                TempData["SuccessMessage"] = $"Artikel '{article.Title}' berhasil dipublikasikan.";
            }
            else
            {
                article.Status = ArticleStatus.Draft;
                article.PublishedAt = null;
                TempData["InfoMessage"] = $"Artikel '{article.Title}' dikembalikan ke draf.";
            }

            article.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Articles/Delete/{id}
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null) return NotFound();

            _fileService.DeleteFile(article.FeaturedImageUrl);
            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin menghapus artikel {ArticleId}: {Title}", id, article.Title);
            TempData["SuccessMessage"] = "Artikel berhasil dihapus permanen oleh admin.";

            return RedirectToAction(nameof(Index));
        }
    }
}
