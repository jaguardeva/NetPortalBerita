using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalBeritaApp.Data;
using PortalBeritaApp.Models;
using PortalBeritaApp.Models.Enums;
using PortalBeritaApp.ViewModels.Admin;

namespace PortalBeritaApp.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Admin
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var totalArticles = await _context.Articles.CountAsync();
            var publishedArticles = await _context.Articles.CountAsync(a => a.Status == ArticleStatus.Published);
            var draftArticles = await _context.Articles.CountAsync(a => a.Status == ArticleStatus.Draft);

            var totalCategories = await _context.Categories.CountAsync();

            var allUsers = await _userManager.Users.ToListAsync();
            var totalUsers = allUsers.Count;
            var activeUsers = allUsers.Count(u => u.IsActive);
            var inactiveUsers = allUsers.Count(u => !u.IsActive);

            var authors = await _userManager.GetUsersInRoleAsync("Author");
            var authorsCount = authors.Count;

            var totalComments = await _context.Comments.CountAsync();
            var totalLikes = await _context.ArticleLikes.CountAsync();

            var recentArticles = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Category)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Select(a => new AdminRecentArticleItem
                {
                    Id = a.Id,
                    Title = a.Title,
                    Slug = a.Slug,
                    AuthorName = a.Author != null ? a.Author.DisplayName : "Anonim",
                    CategoryName = a.Category != null ? a.Category.Name : "Umum",
                    Status = a.Status,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            var recentComments = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Article)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .Select(c => new AdminRecentCommentItem
                {
                    Id = c.Id,
                    ArticleTitle = c.Article != null ? c.Article.Title : "-",
                    ArticleSlug = c.Article != null ? c.Article.Slug : "",
                    UserName = c.User != null ? c.User.DisplayName : "Anonim",
                    Content = c.Content,
                    Status = c.Status,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                TotalArticles = totalArticles,
                PublishedArticles = publishedArticles,
                DraftArticles = draftArticles,
                TotalCategories = totalCategories,
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                InactiveUsers = inactiveUsers,
                AuthorsCount = authorsCount,
                TotalComments = totalComments,
                TotalLikes = totalLikes,
                RecentArticles = recentArticles,
                RecentComments = recentComments
            };

            return View(viewModel);
        }
    }
}
