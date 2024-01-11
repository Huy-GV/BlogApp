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

namespace RazorBlog.Pages.User;

[Authorize]
public class EditModel : RichPageModelBase<EditModel>
{
    private readonly IImageStorage _imageStorage;

    private readonly ILogger<EditModel> _logger;

    public EditModel(RazorBlogDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<EditModel> logger,
        IImageStorage imageStorage) : base(context, userManager, logger)
    {
        _imageStorage = imageStorage;
        _logger = logger;
    }

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
            var (result, imageUri) = await _imageStorage.UploadProfileImageAsync(EditUserViewModel.NewProfilePicture);
            if (result == ServiceResultCode.Success)
            {
                _logger.LogInformation("Deleting previous profile image of user named '{userName}')", user.UserName);
                await _imageStorage.DeleteImage(applicationUser.ProfileImageUri);
                applicationUser.ProfileImageUri = imageUri!;
            }
            else
            {
                return this.NavigateOnResult(result, BadRequest);
            }
        }

        await DbContext.SaveChangesAsync();

        return RedirectToPage("/User/Index", new { userName = EditUserViewModel.UserName });
    }
}