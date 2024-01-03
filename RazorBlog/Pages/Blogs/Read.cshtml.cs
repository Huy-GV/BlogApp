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
public class ReadModel(
    RazorBlogDbContext context,
    UserManager<ApplicationUser> userManager,
    ILogger<ReadModel> logger,
    IUserModerationService userModerationService,
    IPostModerationService postModerationService,
    IBlogContentManager blogContentManager,
    IUserPermissionValidator userPermissionValidator) : RichPageModelBase<ReadModel>(context, userManager, logger)
{
    private readonly IUserModerationService _userModerationService = userModerationService;
    private readonly IPostModerationService _postModerationService = postModerationService;
    private readonly IUserPermissionValidator _userPermissionValidator = userPermissionValidator;
    private readonly IBlogContentManager _blogContentManager = blogContentManager;

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

        var blogAuthor = new
        {
            UserName = blog.AuthorUser?.UserName ?? ReplacementText.DeletedUser,
            // todo: un-hardcode default profile pic
            ProfileImageUri = blog.AuthorUser?.ProfileImageUri ?? "readonly/default.jpg",
            Description = blog.AuthorUser?.Description ?? ReplacementText.DeletedUser
        };

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
            AuthorDescription = blogAuthor.Description,
            AuthorName = blogAuthor.UserName,
            AuthorProfileImageUri = blogAuthor.ProfileImageUri,
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
            IsAuthenticated = this.IsUserAuthenticated()
        };

        return Page();
    }

    public async Task<IActionResult> OnPostHideBlogAsync(int blogId)
    {
        if (!this.IsUserAuthenticated())
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
        if (!this.IsUserAuthenticated())
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