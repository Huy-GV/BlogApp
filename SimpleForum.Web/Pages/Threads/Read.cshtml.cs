using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimpleForum.Core.Communication;
using SimpleForum.Core.Data;
using SimpleForum.Core.Data.Dtos;
using SimpleForum.Core.Models;
using SimpleForum.Core.ReadServices;
using SimpleForum.Core.WriteServices;
using SimpleForum.Web.Extensions;

namespace SimpleForum.Web.Pages.Threads;

[AllowAnonymous]
public class ReadModel : RichPageModelBase<ReadModel>
{
    private readonly IPostModerationService _postModerationService;
    private readonly IUserPermissionValidator _userPermissionValidator;
    private readonly IThreadContentManager _threadContentManager;
    private readonly IThreadReader _threadReader;

    public ReadModel(
        SimpleForumDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<ReadModel> logger,
        IPostModerationService postModerationService,
        IThreadContentManager threadContentManager,
        IUserPermissionValidator userPermissionValidator,
        IThreadReader threadReader) : base(context, userManager, logger)
    {
        _postModerationService = postModerationService;
        _userPermissionValidator = userPermissionValidator;
        _threadContentManager = threadContentManager;
        _threadReader = threadReader;
    }

    [BindProperty(SupportsGet = true)]
    public UserPermissionsDto UserPermissionsDto { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public DetailedThreadDto DetailedThreadDto { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var (result, threadDto) = await _threadReader.GetThreadAsync(id, User.Identity?.Name ?? string.Empty);
        if (result != ServiceResultCode.Success)
        {
            return this.NavigateOnError(result);
        }

        DetailedThreadDto = threadDto!;

        var currentUser = await GetUserOrDefaultAsync();
        var currentUserName = currentUser?.UserName ?? string.Empty;
        var currentUserRoles = currentUser != null
            ? await UserManager.GetRolesAsync(currentUser)
            : [];

        var isUserAllowedToUpdateOrDeletePost = await _userPermissionValidator.IsUserAllowedToUpdateOrDeletePostAsync(
                currentUserName,
                DetailedThreadDto.IsReported,
                DetailedThreadDto.AuthorName);

        var isUserAllowedToReportPost = await _userPermissionValidator.IsUserAllowedToReportPostAsync(currentUserName, DetailedThreadDto.AuthorName);
        UserPermissionsDto = new UserPermissionsDto
        {
            UserName = currentUserName,
            AllowedToReportPost = IsAuthenticated && isUserAllowedToReportPost,
            AllowedToModifyOrDeletePost = IsAuthenticated && isUserAllowedToUpdateOrDeletePost,
            AllowedToCreateComment = IsAuthenticated && await _userPermissionValidator.IsUserAllowedToCreatePostAsync(currentUserName),
        };

        return Page();
    }

    public async Task<IActionResult> OnPostHideThreadAsync(int threadId)
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
            await _postModerationService.ReportThreadAsync(threadId, user.UserName ?? string.Empty),
            () => RedirectToPage("/Threads/Read", new { id = threadId }));
    }

    public async Task<IActionResult> OnPostDeleteThreadAsync(int threadId)
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
            await _threadContentManager.DeleteThreadAsync(threadId, user.UserName ?? string.Empty),
            () => RedirectToPage("/Threads/Index"));
    }
}
