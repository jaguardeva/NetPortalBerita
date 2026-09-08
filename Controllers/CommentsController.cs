using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalBeritaApp.Data;
using PortalBeritaApp.Models;
using PortalBeritaApp.Models.Enums;

namespace PortalBeritaApp.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<CommentsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // POST: /Comments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid articleId, string slug, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                if (IsAjaxRequest())
                    return Unauthorized(new { success = false, message = "Silakan masuk terlebih dahulu." });
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(content) || content.Trim().Length > 2000)
            {
                if (IsAjaxRequest())
                    return BadRequest(new { success = false, message = "Isi komentar wajib diisi dan maksimal 2000 karakter." });

                TempData["ErrorMessage"] = "Isi komentar wajib diisi dan maksimal 2000 karakter.";
                return RedirectToAction("Detail", "Articles", new { slug });
            }

            var article = await _context.Articles
                .FirstOrDefaultAsync(a => a.Id == articleId && a.Status == ArticleStatus.Published);

            if (article == null)
            {
                if (IsAjaxRequest())
                    return NotFound(new { success = false, message = "Artikel tidak ditemukan." });

                return NotFound();
            }

            // Sanitize comment text to prevent XSS
            var sanitizer = new HtmlSanitizer();
            // Allow basic text, strip all scripts and dangerous elements
            var cleanContent = sanitizer.Sanitize(content.Trim());

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                ArticleId = articleId,
                UserId = user.Id,
                Content = cleanContent,
                Status = CommentStatus.Published,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var totalComments = await _context.Comments
                .CountAsync(c => c.ArticleId == articleId && c.Status == CommentStatus.Published);

            _logger.LogInformation("Komentar baru ditambahkan oleh {Email} pada artikel {Slug}", user.Email, slug);

            if (IsAjaxRequest())
            {
                return Json(new
                {
                    success = true,
                    message = "Komentar Anda berhasil dikirim.",
                    comment = new
                    {
                        id = comment.Id,
                        userId = user.Id,
                        userDisplayName = user.DisplayName,
                        content = comment.Content,
                        createdAt = comment.CreatedAt.ToString("dd MMM yyyy, HH:mm", new System.Globalization.CultureInfo("id-ID")) + " WIB",
                        canDelete = true
                    },
                    totalComments
                });
            }

            TempData["SuccessMessage"] = "Komentar berhasil dikirim.";
            return Redirect($"/article/{slug}#commentsSection");
        }

        // POST: /Comments/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, string slug)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                if (IsAjaxRequest())
                    return Unauthorized(new { success = false, message = "Silakan masuk terlebih dahulu." });
                return RedirectToAction("Login", "Account");
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                if (IsAjaxRequest())
                    return NotFound(new { success = false, message = "Komentar tidak ditemukan." });
                return NotFound();
            }

            // Check ownership or Admin role
            bool isAdmin = User.IsInRole("Admin");
            if (comment.UserId != user.Id && !isAdmin)
            {
                if (IsAjaxRequest())
                    return Forbid();
                return Forbid();
            }

            var articleId = comment.ArticleId;
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            var totalComments = await _context.Comments
                .CountAsync(c => c.ArticleId == articleId && c.Status == CommentStatus.Published);

            _logger.LogInformation("Komentar {CommentId} dihapus oleh {Email}", id, user.Email);

            if (IsAjaxRequest())
            {
                return Json(new
                {
                    success = true,
                    message = "Komentar berhasil dihapus.",
                    commentId = id,
                    totalComments
                });
            }

            TempData["SuccessMessage"] = "Komentar berhasil dihapus.";
            return Redirect($"/article/{slug}#commentsSection");
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                || Request.Headers["Accept"].ToString().Contains("application/json");
        }
    }
}
