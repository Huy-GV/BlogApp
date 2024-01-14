using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RazorBlog.Data;
using RazorBlog.Data.Constants;
using RazorBlog.Data.Dtos;
using RazorBlog.Data.ViewModels;
using RazorBlog.Extensions;
using RazorBlog.Models;
using RazorBlog.Services;

namespace RazorBlog.Pages.Blogs;

[AllowAnonymous]
public class ReadModel : RichPageModelBase<ReadModel>
{
    private readonly IPostModerationService _postModerationService;
    private readonly IUserPermissionValidator _userPermissionValidator;
    private readonly IBlogContentManager _blogContentManager;
    private readonly IImageStore _imageStore;
    
    public ReadModel(RazorBlogDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<ReadModel> logger,
        IPostModerationService postModerationService,
        IBlogContentManager blogContentManager,
        IUserPermissionValidator userPermissionValidator,
        IImageStore imageStore) : base(context, userManager, logger)
    {
        _postModerationService = postModerationService;
        _userPermissionValidator = userPermissionValidator;
        _blogContentManager = blogContentManager;
        _imageStore = imageStore;
    }

    [BindProperty]
    public CommentViewModel CreateCommentViewModel { get; set; } = null!;

    [BindProperty]
    public CommentViewModel EditCommentViewModel { get; set; } = null!;

    [BindProperty(SupportsGet = true)] 
    public CurrentUserInfo CurrentUserInfo { get; set; } = null!;

    [BindProperty(SupportsGet = true)] 
    public DetailedBlogDto DetailedBlogDto { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var blog = await DbContext.Blog
            .IgnoreQueryFilters()
            .Include(blog => blog.AuthorUser)
            .FirstOrDefaultAsync(blog => blog.Id == id);

        if (blog == null)
        {
            return NotFound();
        }
        
        DbContext.Blog.Update(blog);
        blog.ViewCount++;
        await DbContext.SaveChangesAsync();
        DetailedBlogDto = new DetailedBlogDto
        {
            Id = blog.Id,
            Introduction = blog.IsHidden ? ReplacementText.HiddenContent : blog.Introduction,
            Title = blog.IsHidden ? ReplacementText.HiddenContent : blog.Title,
            Content = blog.IsHidden ? ReplacementText.HiddenContent : blog.Body,
            CoverImageUri = blog.CoverImageUri,
            CreationTime = blog.CreationTime,
            LastUpdateTime = blog.LastUpdateTime,
            IsHidden = blog.IsHidden,
            AuthorDescription = blog.AuthorUser?.Description ?? ReplacementText.DeletedUser,
            AuthorName = blog.AuthorUser?.UserName ?? ReplacementText.DeletedUser,
            AuthorProfileImageUri = blog.AuthorUser?.ProfileImageUri ?? await _imageStore.GetDefaultProfileImageUriAsync(),
        };

        var currentUser = await GetUserOrDefaultAsync();
        var currentUserName = currentUser?.UserName ?? string.Empty;
        var currentUserRoles = currentUser != null
            ? await UserManager.GetRolesAsync(currentUser)
            : new List<string>();

        CurrentUserInfo = new CurrentUserInfo
        {
            UserName = currentUserName,
            AllowedToHidePost = IsAuthenticated && currentUserRoles
                                            .Intersect(new[] { Roles.AdminRole, Roles.ModeratorRole })
                                            .Any(),
            AllowedToModifyOrDeletePost = IsAuthenticated && await _userPermissionValidator.IsUserAllowedToUpdateOrDeletePostAsync(currentUserName, blog),
            AllowedToCreateComment = IsAuthenticated && await _userPermissionValidator.IsUserAllowedToCreatePostAsync(currentUserName),
        };

        return Page();
    }

    public async Task<IActionResult> OnPostHideBlogAsync(int blogId)
    {
        if (!IsAuthenticated)
        {
            return Challenge();
        }

        var user = await GetUserOrDefaultAsync();
        if (user == null)
        {
            return Forbid();
        }

        return this.NavigateOnResult(
            await _postModerationService.HideBlogAsync(blogId, user.UserName ?? string.Empty),
            () => RedirectToPage("/Blogs/Read", new { id = blogId }));
    }

    public async Task<IActionResult> OnPostDeleteBlogAsync(int blogId)
    {
        if (!IsAuthenticated)
        {
            return Challenge();
        }

        var user = await GetUserOrDefaultAsync();
        if (user == null)
        {
            return Forbid();
        }

        return this.NavigateOnResult(
            await _blogContentManager.DeleteBlogAsync(blogId, user.UserName ?? string.Empty),
            () => RedirectToPage("/Blogs/Index"));
    }
}