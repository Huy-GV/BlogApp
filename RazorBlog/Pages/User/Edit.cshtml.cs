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
public class EditModel(
    RazorBlogDbContext context,
    UserManager<ApplicationUser> userManager,
    ILogger<EditModel> logger,
    IImageStorage imageStorage) : RichPageModelBase<EditModel>(context, userManager, logger)
{
    private readonly IImageStorage _imageStorage = imageStorage;

    private readonly ILogger<EditModel> _logger = logger;

    [BindProperty] public EditUserViewModel EditUserViewModel { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(string? userName)
    {
        if (userName == null)
        {
            return NotFound();
        }

        var user = await UserManager.FindByNameAsync(userName);
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
            UserName = userName,
            Description = user.Description
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await GetUserOrDefaultAsync();
        if (user?.UserName == null || user.UserName != EditUserViewModel.UserName)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var applicationUser = await DbContext.ApplicationUser.FindAsync(user.Id);
        if (applicationUser == null)
        {
            return Forbid();
        }

        DbContext.Users.Update(applicationUser);
        applicationUser.Description = EditUserViewModel.Description;

        if (EditUserViewModel.NewProfilePicture != null)
        {
            await _imageStorage.DeleteImage(applicationUser.ProfileImageUri);
            applicationUser.ProfileImageUri = await UploadProfileImageAsync(EditUserViewModel.NewProfilePicture);
        }

        await DbContext.SaveChangesAsync();

        return RedirectToPage("/User/Index", new { userName = EditUserViewModel.UserName });
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