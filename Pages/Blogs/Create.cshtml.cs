using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RazorBlog.Data;
using RazorBlog.Data.ViewModels;
using RazorBlog.Models;
using RazorBlog.Services;

namespace RazorBlog.Pages.Blogs;

[Authorize]
public class CreateModel(
    RazorBlogDbContext context,
    UserManager<ApplicationUser> userManager,
    ILogger<CreateModel> logger,
    IImageStorage imageService,
    IUserModerationService userModerationService) : BasePageModel<CreateModel>(
context, userManager, logger)
{
    private readonly IImageStorage _imageStorage = imageService;
    private readonly IUserModerationService _userModerationService = userModerationService;

    [BindProperty]
    public BlogViewModel CreateBlogViewModel { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await GetUserOrDefaultAsync();
        if (user?.UserName == null)
        {
            return Page();
        }

        if (await _userModerationService.BanTicketExistsAsync(user.UserName))
        {
            return Forbid();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await GetUserOrDefaultAsync();
        if (user?.UserName == null)
        {
            return Page();
        }

        if (await _userModerationService.BanTicketExistsAsync(user.UserName))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            Logger.LogError("Invalid model state when submitting new blog.");
            return Page();
        }

        try
        {
            var imageName = await _imageStorage.UploadBlogCoverImageAsync(CreateBlogViewModel.CoverImage);
            var utcNow = DateTime.UtcNow;
            var newBlog = new Blog
            {
                CoverImageUri = imageName,
                AppUserId = user.Id,
                CreationTime = utcNow,
                LastUpdateTime = utcNow,
            };

            DbContext.Blog.Add(newBlog).CurrentValues.SetValues(CreateBlogViewModel);
            await DbContext.SaveChangesAsync();

            return RedirectToPage("/Blogs/Read", new { id = newBlog.Id });
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to create blog");
            Logger.LogError(ex.Message);

            return Page();
        }
    }
}