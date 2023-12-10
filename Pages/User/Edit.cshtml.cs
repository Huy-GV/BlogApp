using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RazorBlog.Data;
using RazorBlog.Data.ViewModels;
using RazorBlog.Models;
using RazorBlog.Services;

namespace RazorBlog.Pages.User;

[Authorize]
public class EditModel : BasePageModel<EditModel>
{
    private readonly IImageStorage _imageStorage;

    private readonly ILogger<EditModel> _logger;

    public EditModel(
        RazorBlogDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<EditModel> logger,
        IImageStorage imageStorage) : base(context, userManager, logger)
    {
        _logger = logger;
        _imageStorage = imageStorage;
    }

    [BindProperty] public EditUserViewModel EditUserViewModel { get; set; }

    public async Task<IActionResult> OnGetAsync(string? username)
    {
        if (username == null)
        {
            return NotFound();
        }

        var user = await UserManager.FindByNameAsync(username);
        if (user == null)
        {
            return NotFound();
        }

        if (user.UserName != User.Identity?.Name)
        {
            return Forbid();
        }

        EditUserViewModel = new EditUserViewModel
        {
            UserName = username,
            Description = user.Description
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await UserManager.FindByNameAsync(EditUserViewModel.UserName);
        if (user == null)
        {
            return NotFound();
        }

        if (user.UserName != User.Identity?.Name)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(m => m.Errors)
                .Select(e => e.ErrorMessage);

            foreach (var error in errors)
            {
                _logger.LogError(error);
            }

            return RedirectToPage("/User/Edit", new { username = EditUserViewModel.UserName });
        }

        var applicationUser = await DbContext.ApplicationUser.FindAsync(user.Id);
        DbContext.Users.Update(applicationUser);
        applicationUser.Description = EditUserViewModel.Description;

        if (EditUserViewModel.NewProfilePicture != null)
        {
            await _imageStorage.DeleteImage(applicationUser.ProfileImageUri);
            applicationUser.ProfileImageUri = await UploadProfileImageAsync(EditUserViewModel.NewProfilePicture);
        }

        await DbContext.SaveChangesAsync();

        return RedirectToPage("/User/Index", new { username = EditUserViewModel.UserName });
    }

    private async Task<string> UploadProfileImageAsync(IFormFile image)
    {
        try
        {
            return await _imageStorage.UploadProfileImageAsync(image);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to upload new profile picture: {ex}");
            return Path.Combine("ProfileImage", "default.jpg");
        }
    }
}