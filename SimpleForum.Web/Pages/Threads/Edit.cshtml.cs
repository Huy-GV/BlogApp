using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimpleForum.Core.CommandServices;
using SimpleForum.Core.Communication;
using SimpleForum.Core.Data;
using SimpleForum.Core.Data.ViewModels;
using SimpleForum.Core.Models;
using SimpleForum.Core.QueryServices;
using SimpleForum.Web.Extensions;

namespace SimpleForum.Web.Pages.Threads;

[Authorize]
public class EditModel : RichPageModelBase<EditModel>
{
    private readonly IUserPermissionValidator _userPermissionValidator;
    private readonly IThreadContentManager _threadContentManager;
    private readonly IThreadReader _threadReader;

    public EditModel(SimpleForumDbContext context,
        UserManager<ApplicationUser> userManager,
        IThreadContentManager threadContentManager,
        ILogger<EditModel> logger,
        IUserPermissionValidator userPermissionValidator,
        IThreadReader threadReader) : base(context, userManager, logger)
    {
        _userPermissionValidator = userPermissionValidator;
        _threadReader = threadReader;
        _threadContentManager = threadContentManager;
    }

    [BindProperty]
    public EditThreadViewModel EditThreadViewModel { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? threadId, string? userName)
    {
        if (threadId == null || userName == null)
        {
            return NotFound();
        }

        var user = await GetUserOrDefaultAsync();
        if (user?.UserName != userName)
        {
            return Forbid();
        }

        var (result, thread) = await _threadReader.GetThreadAsync(threadId.Value, user.UserName);
        if (result != ServiceResultCode.Success)
        {
            return this.NavigateOnError(result);
        }
        
        if (!await _userPermissionValidator.IsUserAllowedToUpdateOrDeletePostAsync(
                user.UserName ?? string.Empty,
                thread!.IsReported,
                thread.AuthorUserName))
        {
            return Forbid();
        }

        EditThreadViewModel = new EditThreadViewModel
        {
            Id = thread.Id,
            Title = thread.Title,
            Body = thread.Content,
            Introduction = thread.Introduction
        };

        return Page();
    }

    public async Task<IActionResult> OnPostEditThreadAsync()
    {
        if (!ModelState.IsValid)
        {
            Logger.LogError("Invalid model state when editing thread");
            return Page();
        }

        var user = await GetUserOrDefaultAsync();
        if (user == null)
        {
            return Forbid();
        }

        return this.NavigateOnResult(
            await _threadContentManager.UpdateThreadAsync(EditThreadViewModel, user.UserName ?? string.Empty),
            () => RedirectToPage("/Threads/Read", new { id = EditThreadViewModel.Id }));
    }
}
