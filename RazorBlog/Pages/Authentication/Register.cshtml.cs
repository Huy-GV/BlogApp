using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RazorBlog.Communication;
using RazorBlog.Data;
using RazorBlog.Data.ViewModels;
using RazorBlog.Extensions;
using RazorBlog.Models;
using RazorBlog.Services;

namespace RazorBlog.Pages.Authentication;

[AllowAnonymous]
public class RegisterModel : RichPageModelBase<RegisterModel>
{
    private readonly IImageStorage _imageStorage;
    private readonly ILogger<RegisterModel> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterModel(
        RazorBlogDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<RegisterModel> logger,
        IImageStorage imageStorage) : base (dbContext, userManager, logger)
    {
        _imageStorage = imageStorage;
        _logger = logger;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    public CreateUserViewModel CreateUserViewModel { get; set; } = null!;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var profileImageUri = await _imageStorage.GetDefaultProfileImageUriAsync();
        if (CreateUserViewModel.ProfilePicture is not null)
        {
            var (uploadResult, uri) = await _imageStorage.UploadProfileImageAsync(CreateUserViewModel.ProfilePicture);
            if (uploadResult != ServiceResultCode.Success)
            {
                return this.NavigateOnResult(uploadResult, BadRequest);
            }

            profileImageUri = uri!;
        }
  
        var user = new ApplicationUser
        {
            UserName = CreateUserViewModel.UserName,
            EmailConfirmed = true,
            RegistrationDate = DateTime.Now,
            ProfileImageUri = profileImageUri
        };

        var result = await _userManager.CreateAsync(user, CreateUserViewModel.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            
            return Page();
        }

        _logger.LogInformation("New account created by user: {userName}.", CreateUserViewModel.UserName);
        await _signInManager.SignInAsync(user, false);

        return LocalRedirect(Url.Content("~/"));
    }
}