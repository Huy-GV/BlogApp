using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RazorBlog.Data;
using RazorBlog.Data.ViewModels;
using RazorBlog.Extensions;
using RazorBlog.Models;
using RazorBlog.Services;

namespace RazorBlog.Pages.Blogs;

[Authorize]
public class EditModel : RichPageModelBase<EditModel>
{
    private readonly IUserPermissionValidator _userPermissionValidator;
    private readonly IBlogContentManager _blogContentManager;

    private readonly S3ImageStore _s3ImageStore;

    public EditModel(RazorBlogDbContext context,
        UserManager<ApplicationUser> userManager,
        IBlogContentManager blogContentManager,
        ILogger<EditModel> logger,
        IUserPermissionValidator userPermissionValidator,
        S3ImageStore s3ImageStore) : base(context, userManager, logger)
    {
        _userPermissionValidator = userPermissionValidator;
        _blogContentManager = blogContentManager;
        _s3ImageStore = s3ImageStore;
    }

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

        var blog = await DbContext.Blog.FindAsync(blogId);
        if (blog == null)
        {
            return NotFound();
        }

        if (!await _userPermissionValidator.IsUserAllowedToUpdateOrDeletePostAsync(user.UserName ?? string.Empty, blog))
        {
            return Forbid();
        }

        EditBlogViewModel = new EditBlogViewModel
        {
            Id = blog.Id,
            Title = blog.Title,
            Body = blog.Body,
            Introduction = blog.Introduction
        };

        return Page();
    }

    public async Task<IActionResult> OnPostEditBlogAsync()
    {
        await _s3ImageStore.UploadBlogCoverImageAsync(EditBlogViewModel.CoverImage!);


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

        return this.NavigateOnResult(
            await _blogContentManager.UpdateBlog(EditBlogViewModel, user.UserName ?? string.Empty),
            () => RedirectToPage("/Blogs/Read", new { id = EditBlogViewModel.Id }));
    }
}