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
    public class LikesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LikesController> _logger;

        public LikesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<LikesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // POST: /Likes/Toggle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(Guid articleId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { success = false, message = "Silakan masuk terlebih dahulu." });
            }

            var articleExists = await _context.Articles
                .AnyAsync(a => a.Id == articleId && a.Status == ArticleStatus.Published);

            if (!articleExists)
            {
                return NotFound(new { success = false, message = "Artikel tidak ditemukan." });
            }

            var existingLike = await _context.ArticleLikes
                .FirstOrDefaultAsync(l => l.ArticleId == articleId && l.UserId == user.Id);

            bool isLiked;
            string message;

            if (existingLike != null)
            {
                // Unlike
                _context.ArticleLikes.Remove(existingLike);
                isLiked = false;
                message = "Anda batal menyukai artikel ini.";
            }
            else
            {
                // Like
                var newLike = new ArticleLike
                {
                    Id = Guid.NewGuid(),
                    ArticleId = articleId,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ArticleLikes.Add(newLike);
                isLiked = true;
                message = "Terima kasih, Anda menyukai artikel ini!";
            }

            await _context.SaveChangesAsync();

            var totalLikes = await _context.ArticleLikes.CountAsync(l => l.ArticleId == articleId);

            _logger.LogInformation("Pengguna {Email} {Action} artikel {ArticleId}",
                user.Email, isLiked ? "menyukai" : "batal menyukai", articleId);

            return Json(new
            {
                success = true,
                isLiked,
                totalLikes,
                message
            });
        }
    }
}
