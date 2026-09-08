using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalBeritaApp.Data;
using PortalBeritaApp.Models.Enums;
using PortalBeritaApp.ViewModels.Admin;

namespace PortalBeritaApp.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Comments")]
    public class AdminCommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminCommentsController> _logger;

        public AdminCommentsController(ApplicationDbContext context, ILogger<AdminCommentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Admin/Comments
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(CommentStatus? status = null, string? search = null)
        {
            var query = _context.Comments
                .Include(c => c.Article)
                .Include(c => c.User)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.Trim().ToLower();
                query = query.Where(c => c.Content.ToLower().Contains(lower)
                                      || (c.User != null && c.User.DisplayName.ToLower().Contains(lower))
                                      || (c.Article != null && c.Article.Title.ToLower().Contains(lower)));
            }

            var comments = await query
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new AdminCommentItemViewModel
                {
                    Id = c.Id,
                    ArticleTitle = c.Article != null ? c.Article.Title : "-",
                    ArticleSlug = c.Article != null ? c.Article.Slug : "",
                    UserName = c.User != null ? c.User.DisplayName : "Anonim",
                    UserEmail = c.User != null ? c.User.Email ?? "" : "",
                    Content = c.Content,
                    Status = c.Status,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            var viewModel = new AdminCommentListViewModel
            {
                Comments = comments,
                StatusFilter = status,
                SearchQuery = search
            };

            return View(viewModel);
        }

        // POST: /Admin/Comments/Hide/{id}
        [HttpPost("Hide/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hide(Guid id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            comment.Status = CommentStatus.Hidden;
            comment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin menyembunyikan komentar {Id}", id);
            TempData["SuccessMessage"] = "Komentar berhasil disembunyikan dari tampilan publik.";

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Comments/Restore/{id}
        [HttpPost("Restore/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(Guid id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            comment.Status = CommentStatus.Published;
            comment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin memulihkan komentar {Id}", id);
            TempData["SuccessMessage"] = "Komentar berhasil dipulihkan ke status publik.";

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Comments/Delete/{id}
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin menghapus permanen komentar {Id}", id);
            TempData["SuccessMessage"] = "Komentar berhasil dihapus secara permanen.";

            return RedirectToAction(nameof(Index));
        }
    }
}
