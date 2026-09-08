using Microsoft.AspNetCore.Identity;
using PortalBeritaApp.Models;

namespace PortalBeritaApp.Middleware
{
    public class ActiveUserMiddleware
    {
        private readonly RequestDelegate _next;

        public ActiveUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null || !user.IsActive)
                {
                    await signInManager.SignOutAsync();
                    context.Response.Cookies.Delete(".AspNetCore.Identity.Application");
                    context.Response.Redirect("/Account/Login?inactive=true");
                    return;
                }
            }

            await _next(context);
        }
    }
}
