using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SimpleForum.Core.Models;

namespace SimpleForum.Web.Pages.Authentication;

[AllowAnonymous]
public class LogoutModel : PageModel
{
    private readonly ILogger<LogoutModel> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LogoutModel(
        SignInManager<ApplicationUser> signInManager,
        ILogger<LogoutModel> logger)
    {
        _logger = logger;
        _signInManager = signInManager;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        const string returnUrl = "/Threads/Index";
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");

        return RedirectToPage(returnUrl);
    }
}
