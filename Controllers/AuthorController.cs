using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PortalBeritaApp.Data;
using PortalBeritaApp.Helpers;
using PortalBeritaApp.Models;
using PortalBeritaApp.Models.Enums;
using PortalBeritaApp.Services;
using PortalBeritaApp.ViewModels.Author;

namespace PortalBeritaApp.Controllers
{
    [Authorize(Roles = "Author,Admin")]
    public class AuthorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileService _fileService;
        private readonly ILogger<AuthorController> _logger;

        public AuthorController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IFileService fileService,
            ILogger<AuthorController> logger)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
            _logger = logger;
        }

        // GET: /Author
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, ArticleStatus? status = null, string? search = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            int pageSize = 10;
            if (page < 1) page = 1;

            var query = _context.Articles
                .Include(a => a.Category)
                .Include(a => a.Likes)
                .Include(a => a.Comments)
                .Where(a => a.AuthorId == user.Id);

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var trimmed = search.Trim().ToLower();
                query = query.Where(a => a.Title.ToLower().Contains(trimmed));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var articles = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ArticleItemViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Slug = a.Slug,
                    CategoryName = a.Category != null ? a.Category.Name : "-",
                    FeaturedImageUrl = a.FeaturedImageUrl,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt,
                    PublishedAt = a.PublishedAt,
                    LikesCount = a.Likes.Count,
                    CommentsCount = a.Comments.Count
                })
                .ToListAsync();

            var viewModel = new ArticleListViewModel
            {
                Articles = articles,
                StatusFilter = status,
                SearchQuery = search,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };

            return View(viewModel);
        }

        // GET: /Author/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new ArticleCreateViewModel
            {
                Categories = await GetCategorySelectListAsync()
            };

            return View(viewModel);
        }

        // POST: /Author/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ArticleCreateViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategorySelectListAsync(model.CategoryId);
                return View(model);
            }

            // Generate unique slug
            var baseSlug = SlugHelper.GenerateSlug(model.Title);
            var slug = await GenerateUniqueSlugAsync(baseSlug);

            // Handle image upload
            string? imageUrl = null;
            try
            {
                imageUrl = await _fileService.UploadImageAsync(model.FeaturedImage);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(nameof(model.FeaturedImage), ex.Message);
                model.Categories = await GetCategorySelectListAsync(model.CategoryId);
                return View(model);
            }

            // Sanitize content against XSS
            var sanitizer = new HtmlSanitizer();
            var cleanContent = sanitizer.Sanitize(model.Content);

            var article = new Article
            {
                Id = Guid.NewGuid(),
                AuthorId = user.Id,
                CategoryId = model.CategoryId!.Value,
                Title = model.Title.Trim(),
                Slug = slug,
                Excerpt = model.Excerpt.Trim(),
                Content = cleanContent,
                FeaturedImageUrl = imageUrl,
                Status = model.Status,
                PublishedAt = model.Status == ArticleStatus.Published ? DateTime.UtcNow : null,
                CreatedAt = DateTime.UtcNow
            };

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Artikel baru dibuat: {Title} (ID: {Id}) oleh {Author}", article.Title, article.Id, user.Email);
            TempData["SuccessMessage"] = model.Status == ArticleStatus.Published
                ? "Artikel berhasil dipublikasikan!"
                : "Artikel berhasil disimpan sebagai draf.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Author/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            // Check ownership (or Admin)
            if (article.AuthorId != user.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var viewModel = new ArticleEditViewModel
            {
                Id = article.Id,
                Slug = article.Slug,
                Title = article.Title,
                CategoryId = article.CategoryId,
                Excerpt = article.Excerpt ?? string.Empty,
                Content = article.Content,
                ExistingFeaturedImageUrl = article.FeaturedImageUrl,
                Status = article.Status,
                Categories = await GetCategorySelectListAsync(article.CategoryId)
            };

            return View(viewModel);
        }

        // POST: /Author/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ArticleEditViewModel model)
        {
            if (id != model.Id) return BadRequest();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var article = await _context.Articles.FindAsync(id);
            if (article == null) return NotFound();

            // Check ownership (or Admin)
            if (article.AuthorId != user.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                model.ExistingFeaturedImageUrl = article.FeaturedImageUrl;
                model.Slug = article.Slug;
                model.Categories = await GetCategorySelectListAsync(model.CategoryId);
                return View(model);
            }

            // Handle Slug regeneration if title changed
            if (!string.Equals(article.Title, model.Title.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                var baseSlug = SlugHelper.GenerateSlug(model.Title);
                article.Slug = await GenerateUniqueSlugAsync(baseSlug, article.Id);
            }

            // Handle new image upload
            if (model.NewFeaturedImage != null)
            {
                try
                {
                    var newImageUrl = await _fileService.UploadImageAsync(model.NewFeaturedImage);
                    if (!string.IsNullOrEmpty(newImageUrl))
                    {
                        // Remove old image
                        _fileService.DeleteFile(article.FeaturedImageUrl);
                        article.FeaturedImageUrl = newImageUrl;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(nameof(model.NewFeaturedImage), ex.Message);
                    model.ExistingFeaturedImageUrl = article.FeaturedImageUrl;
                    model.Slug = article.Slug;
                    model.Categories = await GetCategorySelectListAsync(model.CategoryId);
                    return View(model);
                }
            }

            // Sanitize content
            var sanitizer = new HtmlSanitizer();
            article.Content = sanitizer.Sanitize(model.Content);

            article.Title = model.Title.Trim();
            article.Excerpt = model.Excerpt.Trim();
            article.CategoryId = model.CategoryId!.Value;

            // Handle status change
            if (article.Status != model.Status)
            {
                if (model.Status == ArticleStatus.Published && article.PublishedAt == null)
                {
                    article.PublishedAt = DateTime.UtcNow;
                }
                else if (model.Status == ArticleStatus.Draft)
                {
                    article.PublishedAt = null;
                }
                article.Status = model.Status;
            }

            article.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Artikel diperbarui: {Title} (ID: {Id})", article.Title, article.Id);
            TempData["SuccessMessage"] = "Artikel berhasil diperbarui!";

            return RedirectToAction(nameof(Index));
        }

        // POST: /Author/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var article = await _context.Articles.FindAsync(id);
            if (article == null) return NotFound();

            if (article.AuthorId != user.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Delete associated image
            _fileService.DeleteFile(article.FeaturedImageUrl);

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Artikel dihapus: {Title} (ID: {Id})", article.Title, article.Id);
            TempData["SuccessMessage"] = "Artikel berhasil dihapus.";

            return RedirectToAction(nameof(Index));
        }

        // POST: /Author/ToggleStatus/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var article = await _context.Articles.FindAsync(id);
            if (article == null) return NotFound();

            if (article.AuthorId != user.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (article.Status == ArticleStatus.Draft)
            {
                article.Status = ArticleStatus.Published;
                if (article.PublishedAt == null)
                {
                    article.PublishedAt = DateTime.UtcNow;
                }
                TempData["SuccessMessage"] = "Artikel berhasil dipublikasikan!";
            }
            else
            {
                article.Status = ArticleStatus.Draft;
                article.PublishedAt = null;
                TempData["InfoMessage"] = "Artikel dikembalikan ke status draf.";
            }

            article.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<SelectListItem>> GetCategorySelectListAsync(Guid? selectedId = null)
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = selectedId.HasValue && c.Id == selectedId.Value
                })
                .ToListAsync();
        }

        private async Task<string> GenerateUniqueSlugAsync(string baseSlug, Guid? excludeArticleId = null)
        {
            var slug = baseSlug;
            int counter = 1;

            while (true)
            {
                var query = _context.Articles.Where(a => a.Slug == slug);
                if (excludeArticleId.HasValue)
                {
                    query = query.Where(a => a.Id != excludeArticleId.Value);
                }

                var exists = await query.AnyAsync();
                if (!exists)
                {
                    return slug;
                }

                slug = $"{baseSlug}-{counter}";
                counter++;
            }
        }
    }
}
