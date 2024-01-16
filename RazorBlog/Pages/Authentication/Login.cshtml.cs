using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using RazorBlog.Data.ViewModels;
using RazorBlog.Models;

namespace RazorBlog.Pages.Authentication;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly ILogger<LoginModel> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        ILogger<LoginModel> logger)
    {
        _logger = logger;
        _signInManager = signInManager;
    }

    [BindProperty] 
    public LogInViewModel LogInViewModel { get; set; } = null!;

    public string? ReturnUrl { get; set; }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        Console.WriteLine("Return URL from log in model: " + returnUrl);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(
            LogInViewModel.UserName,
            LogInViewModel.Password,
            LogInViewModel.RememberMe,
            false);

        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in.");
            return LocalRedirect(returnUrl);
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out.");
            return Page();
        }

        ModelState.AddModelError(string.Empty, "Incorrect username or password.");
        return Page();
    }
}