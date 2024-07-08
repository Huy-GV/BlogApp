using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RazorBlog.Core.Communication;
using RazorBlog.Core.Data;
using RazorBlog.Core.Data.ViewModels;
using RazorBlog.Core.Models;
using RazorBlog.Core.Services;
using RazorBlog.Web.Extensions;

namespace RazorBlog.Web.Pages.User;

[Authorize]
public class EditModel : RichPageModelBase<EditModel>
{
    private readonly IImageStore _imageStore;

    private readonly ILogger<EditModel> _logger;

    public EditModel(RazorBlogDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<EditModel> logger,
        IImageStore imageStore) : base(context, userManager, logger)
    {
        _imageStore = imageStore;
        _logger = logger;
    }

    [BindProperty]
    public EditUserViewModel EditUserViewModel { get; set; } = null!;

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

        applicationUser.Description = EditUserViewModel.Description;
        if (EditUserViewModel.NewProfilePicture != null)
        {
            var (result, imageUri) = await _imageStore.UploadProfileImageAsync(EditUserViewModel.NewProfilePicture);
            if (result == ServiceResultCode.Success)
            {
                _logger.LogInformation("Deleting previous profile image of user named '{userName}')", user.UserName);
                await _imageStore.DeleteImage(applicationUser.ProfileImageUri);
                applicationUser.ProfileImageUri = imageUri!;
            }
            else
            {
                return this.NavigateOnError(result);
            }
        }

        await DbContext.SaveChangesAsync();

        return RedirectToPage("/User/Index", new { userName = EditUserViewModel.UserName });
    }
}
