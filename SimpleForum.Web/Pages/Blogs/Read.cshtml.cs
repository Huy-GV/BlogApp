using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimpleForum.Core.Communication;
using SimpleForum.Core.Data;
using SimpleForum.Core.Data.Constants;
using SimpleForum.Core.Data.Dtos;
using SimpleForum.Core.Models;
using SimpleForum.Core.ReadServices;
using SimpleForum.Core.WriteServices;
using SimpleForum.Web.Extensions;

namespace SimpleForum.Web.Pages.Blogs;

[AllowAnonymous]
public class ReadModel : RichPageModelBase<ReadModel>
{
    private readonly IPostModerationService _postModerationService;
    private readonly IUserPermissionValidator _userPermissionValidator;
    private readonly IBlogContentManager _blogContentManager;
    private readonly IBlogReader _blogReader;

    public ReadModel(
        SimpleForumDbContext context,
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
            : [];

        CurrentUserInfo = new CurrentUserInfo
        {
            UserName = currentUserName,
            AllowedToHidePost = IsAuthenticated && currentUserRoles
                                            .Intersect([Roles.AdminRole, Roles.ModeratorRole])
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
