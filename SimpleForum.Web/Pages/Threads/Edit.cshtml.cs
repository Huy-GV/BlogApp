using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimpleForum.Core.Data;
using SimpleForum.Core.Data.ViewModels;
using SimpleForum.Core.Models;
using SimpleForum.Core.ReadServices;
using SimpleForum.Core.WriteServices;
using SimpleForum.Web.Extensions;

namespace SimpleForum.Web.Pages.Threads;

[Authorize]
public class EditModel : RichPageModelBase<EditModel>
{
    private readonly IUserPermissionValidator _userPermissionValidator;
    private readonly IThreadContentManager _threadContentManager;

    public EditModel(SimpleForumDbContext context,
        UserManager<ApplicationUser> userManager,
        IThreadContentManager threadContentManager,
        ILogger<EditModel> logger,
        IUserPermissionValidator userPermissionValidator) : base(context, userManager, logger)
    {
        _userPermissionValidator = userPermissionValidator;
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

        var thread = await DbContext.Thread.FindAsync(threadId);
        if (thread == null)
        {
            return NotFound();
        }

        if (!await _userPermissionValidator.IsUserAllowedToUpdateOrDeletePostAsync(
                user.UserName ?? string.Empty,
                thread.IsHidden,
                thread.AuthorUserName))
        {
            return Forbid();
        }

        EditThreadViewModel = new EditThreadViewModel
        {
            Id = thread.Id,
            Title = thread.Title,
            Body = thread.Body,
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
