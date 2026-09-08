using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalBeritaApp.Data;
using PortalBeritaApp.Models;
using PortalBeritaApp.ViewModels.Admin;

namespace PortalBeritaApp.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Users")]
    public class AdminUsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminUsersController> _logger;

        public AdminUsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            ApplicationDbContext context,
            ILogger<AdminUsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        // GET: /Admin/Users
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? role = null, string? search = null)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.Trim().ToLower();
                query = query.Where(u => (u.Email != null && u.Email.ToLower().Contains(lower))
                                      || u.DisplayName.ToLower().Contains(lower));
            }

            var usersList = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
            var userViewModels = new List<AdminUserItemViewModel>();

            foreach (var user in usersList)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var currentRole = roles.FirstOrDefault() ?? "User";

                if (!string.IsNullOrEmpty(role) && !string.Equals(currentRole, role, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var articlesCount = await _context.Articles.CountAsync(a => a.AuthorId == user.Id);
                var commentsCount = await _context.Comments.CountAsync(c => c.UserId == user.Id);

                userViewModels.Add(new AdminUserItemViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    DisplayName = user.DisplayName,
                    CurrentRole = currentRole,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    ArticlesCount = articlesCount,
                    CommentsCount = commentsCount
                });
            }

            var allRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();

            var viewModel = new AdminUserListViewModel
            {
                Users = userViewModels,
                RoleFilter = role,
                SearchQuery = search,
                AllRoles = allRoles
            };

            return View(viewModel);
        }

        // POST: /Admin/Users/ChangeRole
        [HttpPost("ChangeRole")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(Guid id, string newRole)
        {
            var validRoles = new[] { "Admin", "Author", "User" };
            if (!validRoles.Contains(newRole))
            {
                TempData["ErrorMessage"] = "Peran yang dipilih tidak valid.";
                return RedirectToAction(nameof(Index));
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && currentUser.Id == id && newRole != "Admin")
            {
                TempData["ErrorMessage"] = "Anda tidak dapat mencabut peran Administrator dari akun Anda sendiri.";
                return RedirectToAction(nameof(Index));
            }

            var targetUser = await _userManager.FindByIdAsync(id.ToString());
            if (targetUser == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(targetUser);
            await _userManager.RemoveFromRolesAsync(targetUser, currentRoles);
            await _userManager.AddToRoleAsync(targetUser, newRole);

            _logger.LogInformation("Admin {Admin} mengubah peran pengguna {User} menjadi {Role}",
                currentUser?.Email, targetUser.Email, newRole);

            TempData["SuccessMessage"] = $"Peran {targetUser.DisplayName} berhasil diubah menjadi '{newRole}'.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Users/ToggleStatus/{id}
        [HttpPost("ToggleStatus/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && currentUser.Id == id)
            {
                TempData["ErrorMessage"] = "Anda tidak dapat menonaktifkan akun Anda sendiri.";
                return RedirectToAction(nameof(Index));
            }

            var targetUser = await _userManager.FindByIdAsync(id.ToString());
            if (targetUser == null) return NotFound();

            targetUser.IsActive = !targetUser.IsActive;
            targetUser.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(targetUser);

            _logger.LogInformation("Admin {Admin} mengubah status aktif {User} menjadi {IsActive}",
                currentUser?.Email, targetUser.Email, targetUser.IsActive);

            TempData["SuccessMessage"] = targetUser.IsActive
                ? $"Akun {targetUser.DisplayName} berhasil diaktifkan kembali."
                : $"Akun {targetUser.DisplayName} telah dinonaktifkan.";

            return RedirectToAction(nameof(Index));
        }
    }
}
