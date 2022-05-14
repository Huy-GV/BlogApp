using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using RazorBlog.Data.ViewModels;
using RazorBlog.Models;
using RazorBlog.Services;

namespace RazorBlog.Pages.Authentication;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly IImageStorage _imageStorage;
    private readonly ILogger<RegisterModel> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<RegisterModel> logger,
        IImageStorage imageStorage)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _imageStorage = imageStorage;
    }

    [BindProperty] public CreateUserViewModel CreateUserViewModel { get; set; }

    public string ReturnUrl { get; set; }

    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    public void OnGet(string returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        // returnUrl ??= Url.Content("~/");
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = CreateUserViewModel.UserName,
            EmailConfirmed = true,
            RegistrationDate = DateTime.Now,
            ProfileImageUri = CreateUserViewModel.ProfilePicture == null
                ? GetDefaultProfileImageUri()
                : await UploadProfileImage(CreateUserViewModel.ProfilePicture)
        };

        var result = await _userManager.CreateAsync(user, CreateUserViewModel.Password);
        if (!result.Succeeded) return Page();

        _logger.LogInformation($"User created a new account with username {CreateUserViewModel.UserName}.");
        await _signInManager.SignInAsync(user, false);

        return LocalRedirect(returnUrl);
    }

    private async Task<string> UploadProfileImage(IFormFile image)
    {
        try
        {
            return await _imageStorage.UploadProfileImageAsync(image);
        }
        catch (Exception ex)
        {
            // todo: un-hardcode the default image path
            _logger.LogError($"Failed to upload new profile picture: {ex}");
            return GetDefaultProfileImageUri();
        }
    }

    private string GetDefaultProfileImageUri()
    {
        return Path.Combine("ProfileImage", "default.jpg");
    }
}