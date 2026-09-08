using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalBeritaApp.Data;
using PortalBeritaApp.Models;
using PortalBeritaApp.ViewModels.Account;

namespace PortalBeritaApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Email ini sudah terdaftar. Silakan masuk atau gunakan email lain.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = model.Email,
                Email = model.Email,
                DisplayName = model.DisplayName.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("Pengguna baru terdaftar: {Email} (ID: {UserId})", user.Email, user.Id);

                // Default role is User (PRD §8)
                await _userManager.AddToRoleAsync(user, "User");

                // Sign the user in
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["SuccessMessage"] = $"Selamat datang di NewsPortal, {user.DisplayName}!";

                return RedirectToLocal(returnUrl);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null, bool inactive = false)
        {
            if (User.Identity?.IsAuthenticated == true && !inactive)
            {
                return RedirectToAction("Index", "Home");
            }

            if (inactive)
            {
                ViewBag.InactiveAlert = "Sesi Anda telah berakhir karena akun Anda dinonaktifkan oleh administrator.";
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            returnUrl ??= model.ReturnUrl;
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email atau kata sandi tidak valid.");
                return View(model);
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Percobaan login ke akun non-aktif: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Akun Anda telah dinonaktifkan oleh administrator. Silakan hubungi admin.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("Pengguna berhasil login: {Email}", user.Email);
                TempData["SuccessMessage"] = $"Selamat datang kembali, {user.DisplayName}!";
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Akun pengguna terkunci: {Email}", user.Email);
                ModelState.AddModelError(string.Empty, "Akun Anda terkunci sementara karena beberapa kali gagal masuk. Silakan tunggu 5 menit.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Email atau kata sandi tidak valid.");
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Pengguna keluar: {Email}", user?.Email);
            TempData["InfoMessage"] = "Anda telah berhasil keluar.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Profile
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var roles = await _userManager.GetRolesAsync(user);

            var articlesCount = await _context.Articles.CountAsync(a => a.AuthorId == user.Id);
            var commentsCount = await _context.Comments.CountAsync(c => c.UserId == user.Id);
            var likesCount = await _context.ArticleLikes.CountAsync(l => l.UserId == user.Id);

            var model = new ProfileViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                DisplayName = user.DisplayName,
                Roles = roles,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive,
                ArticlesCount = articlesCount,
                CommentsCount = commentsCount,
                LikesCount = likesCount
            };

            return View(model);
        }

        // POST: /Account/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (string.IsNullOrWhiteSpace(model.DisplayName) || model.DisplayName.Trim().Length < 2 || model.DisplayName.Trim().Length > 50)
            {
                ModelState.AddModelError(nameof(model.DisplayName), "Nama tampilan harus antara 2 dan 50 karakter.");
            }

            if (!ModelState.IsValid)
            {
                // Reload counts & roles
                model.Email = user.Email ?? string.Empty;
                model.Roles = await _userManager.GetRolesAsync(user);
                model.CreatedAt = user.CreatedAt;
                model.IsActive = user.IsActive;
                model.ArticlesCount = await _context.Articles.CountAsync(a => a.AuthorId == user.Id);
                model.CommentsCount = await _context.Comments.CountAsync(c => c.UserId == user.Id);
                model.LikesCount = await _context.ArticleLikes.CountAsync(l => l.UserId == user.Id);
                return View("Profile", model);
            }

            user.DisplayName = model.DisplayName.Trim();
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profil Anda berhasil diperbarui.";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.Email = user.Email ?? string.Empty;
            model.Roles = await _userManager.GetRolesAsync(user);
            model.CreatedAt = user.CreatedAt;
            model.IsActive = user.IsActive;
            return View("Profile", model);
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Data perubahan kata sandi tidak valid.";
                return RedirectToAction(nameof(Profile));
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                _logger.LogInformation("Pengguna {Email} telah mengganti kata sandi.", user.Email);
                TempData["SuccessMessage"] = "Kata sandi Anda berhasil diubah.";
                return RedirectToAction(nameof(Profile));
            }

            TempData["ErrorMessage"] = string.Join(" ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Profile));
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
