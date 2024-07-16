using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimpleForum.Core.Communication;
using SimpleForum.Core.Data;
using SimpleForum.Core.Data.ViewModels;
using SimpleForum.Core.Models;
using SimpleForum.Core.WriteServices;
using SimpleForum.Web.Extensions;

namespace SimpleForum.Web.Pages.Authentication;

[AllowAnonymous]
public class RegisterModel : RichPageModelBase<RegisterModel>
{
    private readonly IImageStore _imageStore;
    private readonly ILogger<RegisterModel> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterModel(
        SimpleForumDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<RegisterModel> logger,
        IImageStore imageStore) : base(dbContext, userManager, logger)
    {
        _imageStore = imageStore;
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

        var profileImageUri = await _imageStore.GetDefaultProfileImageUriAsync();
        if (CreateUserViewModel.ProfilePicture is not null)
        {
            var (uploadResult, uri) = await _imageStore.UploadProfileImageAsync(CreateUserViewModel.ProfilePicture);
            if (uploadResult != ServiceResultCode.Success)
            {
                return this.NavigateOnError(uploadResult);
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
