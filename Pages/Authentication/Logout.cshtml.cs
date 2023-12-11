using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using RazorBlog.Models;

namespace RazorBlog.Pages.Authentication;

[AllowAnonymous]
public class LogoutModel(
    SignInManager<ApplicationUser> signInManager,
    ILogger<LogoutModel> logger) : PageModel
{
    private readonly ILogger<LogoutModel> _logger = logger;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        var returnUrl = "/Blogs/Index";
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");

        return RedirectToPage(returnUrl);
    }
}