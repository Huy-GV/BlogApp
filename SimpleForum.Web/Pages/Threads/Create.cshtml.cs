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
public class CreateModel : RichPageModelBase<CreateModel>
{
    private readonly IUserPermissionValidator _userPermissionValidator;
    private readonly IThreadContentManager _threadContentManager;

    public CreateModel(
        SimpleForumDbContext context,
        UserManager<ApplicationUser> userManager,
        IThreadContentManager threadContentManager,
        ILogger<CreateModel> logger,
        IUserPermissionValidator userPermissionValidator) : base(context, userManager, logger)
    {
        _userPermissionValidator = userPermissionValidator;
        _threadContentManager = threadContentManager;
    }

    [BindProperty]
    public CreateThreadViewModel CreateThreadViewModel { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await GetUserOrDefaultAsync();
        if (user?.UserName == null)
        {
            return Forbid();
        }

        if (!await _userPermissionValidator.IsUserAllowedToCreatePostAsync(user.UserName))
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
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            Logger.LogError("Invalid model state when submitting new thread.");
            return Page();
        }

        var (result, newThreadId) = await _threadContentManager.CreateThreadAsync(CreateThreadViewModel, user.UserName);

        return this.NavigateOnResult(
            result,
            () => RedirectToPage("/Threads/Read", new { id = newThreadId }));
    }
}
