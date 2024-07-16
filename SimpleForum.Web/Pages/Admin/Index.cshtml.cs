using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SimpleForum.Core.Data;
using SimpleForum.Core.Data.Constants;
using SimpleForum.Core.Data.Dtos;
using SimpleForum.Core.Models;

namespace SimpleForum.Web.Pages.Admin;

[Authorize(Roles = Roles.AdminRole)]
public class IndexModel : RichPageModelBase<IndexModel>
{
    public IndexModel(SimpleForumDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<IndexModel> logger) : base(context, userManager, logger)
    {
    }

    [BindProperty]
    public List<UserProfileDto> Moderators { get; set; } = [];

    [BindProperty]
    public List<UserProfileDto> NormalUsers { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var users = await UserManager.Users
            .AsNoTracking()
            .Select(x => new UserProfileDto
            {
                UserName = x.UserName!,
                RegistrationDate = x.RegistrationDate == null
                    ? "a long time ago"
                    : x.RegistrationDate.Value.ToString(@"d/M/yyy"),
            })
            .ToListAsync();

        var moderators = await UserManager.GetUsersInRoleAsync(Roles.ModeratorRole);
        var moderatorUserNames = moderators.Select(x => x.UserName).ToHashSet();

        var admins = await UserManager.GetUsersInRoleAsync(Roles.AdminRole);
        var adminUserNames = admins.Select(x => x.UserName).ToHashSet();

        foreach (var user in users)
        {
            if (!moderatorUserNames.Contains(user.UserName) && !adminUserNames.Contains(user.UserName))
            {
                NormalUsers.Add(user);
                continue;
            }

            if (moderatorUserNames.Contains(user.UserName))
            {
                Moderators.Add(user);
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostRemoveModeratorRoleAsync(string userName)
    {
        var user = await DbContext.Users.FirstOrDefaultAsync(user => user.UserName == userName);
        if (user == null)
        {
            Logger.LogError("User named {userName} not found", userName);
            return Page();
        }

        await UserManager.RemoveFromRoleAsync(user, Roles.ModeratorRole);

        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostAssignModeratorRoleAsync(string userName)
    {
        var user = await DbContext.Users.FirstOrDefaultAsync(user => user.UserName == userName);
        if (user == null)
        {
            Logger.LogError("User named {userName} not found", userName);
            return Page();
        }

        await UserManager.AddToRoleAsync(user, Roles.ModeratorRole);
        return RedirectToPage("Index");
    }
}
