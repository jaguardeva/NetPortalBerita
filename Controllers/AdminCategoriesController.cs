using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalBeritaApp.Data;
using PortalBeritaApp.Helpers;
using PortalBeritaApp.Models;
using PortalBeritaApp.ViewModels.Admin;

namespace PortalBeritaApp.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Categories")]
    public class AdminCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminCategoriesController> _logger;

        public AdminCategoriesController(ApplicationDbContext context, ILogger<AdminCategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Admin/Categories
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new AdminCategoryItemViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    ArticleCount = c.Articles.Count,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return View(categories);
        }

        // GET: /Admin/Categories/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View(new CategoryCreateEditViewModel());
        }

        // POST: /Admin/Categories/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var trimmedName = model.Name.Trim();
            var slug = SlugHelper.GenerateSlug(trimmedName);

            // Check duplicate name or slug
            var duplicate = await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == trimmedName.ToLower() || c.Slug == slug);

            if (duplicate)
            {
                ModelState.AddModelError(nameof(model.Name), "Nama kategori atau slug ini sudah digunakan.");
                return View(model);
            }

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = trimmedName,
                Slug = slug,
                Description = model.Description?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Kategori baru dibuat oleh admin: {Name} ({Slug})", category.Name, category.Slug);
            TempData["SuccessMessage"] = $"Kategori '{category.Name}' berhasil dibuat.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Categories/Edit/{id}
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            var viewModel = new CategoryCreateEditViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return View(viewModel);
        }

        // POST: /Admin/Categories/Edit/{id}
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CategoryCreateEditViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            var trimmedName = model.Name.Trim();
            var newSlug = SlugHelper.GenerateSlug(trimmedName);

            // Check duplicate name or slug on OTHER categories
            var duplicate = await _context.Categories
                .AnyAsync(c => c.Id != id && (c.Name.ToLower() == trimmedName.ToLower() || c.Slug == newSlug));

            if (duplicate)
            {
                ModelState.AddModelError(nameof(model.Name), "Nama kategori atau slug ini sudah digunakan oleh kategori lain.");
                return View(model);
            }

            category.Name = trimmedName;
            category.Slug = newSlug;
            category.Description = model.Description?.Trim();
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Kategori diperbarui: {Name} (ID: {Id})", category.Name, category.Id);
            TempData["SuccessMessage"] = $"Kategori '{category.Name}' berhasil diperbarui.";

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Categories/Delete/{id}
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var category = await _context.Categories
                .Include(c => c.Articles)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            // PRD §50 constraint: Cannot delete category with associated articles
            if (category.Articles.Any())
            {
                TempData["ErrorMessage"] = $"Kategori '{category.Name}' tidak dapat dihapus karena masih memiliki {category.Articles.Count} artikel terkait.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Kategori dihapus oleh admin: {Name}", category.Name);
            TempData["SuccessMessage"] = $"Kategori '{category.Name}' berhasil dihapus.";

            return RedirectToAction(nameof(Index));
        }
    }
}
