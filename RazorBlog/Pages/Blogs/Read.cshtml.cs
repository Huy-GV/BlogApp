using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RazorBlog.Core.Communication;
using RazorBlog.Core.Data;
using RazorBlog.Core.Data.Constants;
using RazorBlog.Core.Data.Dtos;
using RazorBlog.Extensions;
using RazorBlog.Core.Models;
using RazorBlog.Core.Services;

namespace RazorBlog.Pages.Blogs;

[AllowAnonymous]
public class ReadModel : RichPageModelBase<ReadModel>
{
    private readonly IPostModerationService _postModerationService;
    private readonly IUserPermissionValidator _userPermissionValidator;
    private readonly IBlogContentManager _blogContentManager;
    private readonly IBlogReader _blogReader;

    public ReadModel(
        RazorBlogDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<ReadModel> logger,
        IPostModerationService postModerationService,
        IBlogContentManager blogContentManager,
        IUserPermissionValidator userPermissionValidator,
        IBlogReader blogReader) : base(context, userManager, logger)
    {
        _postModerationService = postModerationService;
        _userPermissionValidator = userPermissionValidator;
        _blogContentManager = blogContentManager;
        _blogReader = blogReader;
    }

    [BindProperty(SupportsGet = true)]
    public CurrentUserInfo CurrentUserInfo { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public DetailedBlogDto DetailedBlogDto { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var (result, blogDto) = await _blogReader.GetBlogAsync(id);
        if (result != ServiceResultCode.Success)
        {
            return this.NavigateOnError(result);
        }

        DetailedBlogDto = blogDto!;

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
            AllowedToModifyOrDeletePost = IsAuthenticated && await _userPermissionValidator.IsUserAllowedToUpdateOrDeletePostAsync(
                currentUserName,
                DetailedBlogDto.IsHidden,
                DetailedBlogDto.AuthorName),
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
