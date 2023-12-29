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
using RazorBlog.Models;

namespace RazorBlog.Pages.Admin;

[Authorize(Roles = Roles.AdminRole)]
public class AdminModel(RazorBlogDbContext context,
    UserManager<ApplicationUser> userManager,
    ILogger<AdminModel> logger) : BasePageModel<AdminModel>(context, userManager, logger)
{
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

    public async Task<IActionResult> OnPostRemoveModeratorRoleAsync(string username)
    {
        var user = await DbContext.Users.FirstOrDefaultAsync(user => user.UserName == username);
        if (user == null)
        {
            Logger.LogError($"No user with ID {username} was found");
            return Page();
        }

        await UserManager.RemoveFromRoleAsync(user, Roles.ModeratorRole);

        return RedirectToPage("Admin");
    }

    public async Task<IActionResult> OnPostAssignModeratorRoleAsync(string username)
    {
        var user = await DbContext.Users.FirstOrDefaultAsync(user => user.UserName == username);
        if (user == null)
        {
            Logger.LogError($"No user with ID {username} was found");
            return Page();
        }

        await UserManager.AddToRoleAsync(user, Roles.ModeratorRole);
        return RedirectToPage("Admin");
    }
}