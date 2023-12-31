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
public class EditModel(
    RazorBlogDbContext context,
    UserManager<ApplicationUser> userManager,
    ILogger<EditModel> logger,
    IImageStorage imageStorage,
    IUserModerationService userModerationService) : RichPageModelBase<EditModel>(context, userManager, logger)
{
    private readonly IImageStorage _imageStorage = imageStorage;
    private readonly IUserModerationService _userModerationService = userModerationService;

    [BindProperty]
    public EditBlogViewModel EditBlogViewModel { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? blogId, string? userName)
    {
        if (blogId == null || userName == null)
        {
            return NotFound();
        }

        var user = await GetUserOrDefaultAsync();
        if (user?.UserName != userName)
        {
            return Forbid();
        }

        if (await _userModerationService.BanTicketExistsAsync(user.UserName ?? string.Empty))
        {
            return Forbid();
        }

        var blog = await DbContext.Blog.FindAsync(blogId);

        if (blog == null)
        {
            return NotFound();
        }

        EditBlogViewModel = new EditBlogViewModel
        {
            Id = blog.Id,
            Title = blog.Title,
            Content = blog.Content,
            Introduction = blog.Introduction
        };

        return Page();
    }

    public async Task<IActionResult> OnPostEditBlogAsync()
    {
        if (!ModelState.IsValid)
        {
            Logger.LogError("Invalid model state when editing blog");
            return Page();
        }

        var user = await GetUserOrDefaultAsync();
        if (user == null)
        {
            return Forbid();
        }

        if (await _userModerationService.BanTicketExistsAsync(user.UserName ?? string.Empty))
        {
            return Forbid();
        }

        var blog = await DbContext.Blog.FindAsync(EditBlogViewModel.Id);
        if (blog == null)
        {
            return NotFound();
        }

        if (string.IsNullOrEmpty(EditBlogViewModel.Content))
        {
            return RedirectToPage("/Blogs/Read", new { id = blog.Id });
        }

        if (user.UserName != blog.AppUser.UserName)
        {
            return Forbid();
        }

        blog.LastUpdateTime = DateTime.UtcNow;
        DbContext.Blog.Update(blog).CurrentValues.SetValues(EditBlogViewModel);

        if (EditBlogViewModel.CoverImage != null)
        {
            try
            {
                await _imageStorage.DeleteImage(blog.CoverImageUri);
                var imageName = await _imageStorage.UploadBlogCoverImageAsync(EditBlogViewModel.CoverImage);
                blog.CoverImageUri = imageName;
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to update blog image");
                Logger.LogError(ex.Message);
            }
        }

        await DbContext.SaveChangesAsync();
        return RedirectToPage("/Blogs/Read", new { id = blog.Id });
    }
}