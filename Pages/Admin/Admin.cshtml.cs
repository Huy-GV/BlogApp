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
using RazorBlog.Data.DTOs;
using RazorBlog.Models;

namespace RazorBlog.Pages.Admin;

//PASSWORD: Admin123@@
[Authorize(Roles = "admin")]
public class AdminModel : BasePageModel<AdminModel>
{
    public AdminModel(RazorBlogDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<AdminModel> logger) : base(context, userManager, logger)
    {
    }

    //TODO: add a filter that shows moderators only
    public async Task<IActionResult> OnGetAsync()
    {
        var adminUser = await GetUserAsync();
        var users = UserManager.Users
            .AsNoTracking()
            .Where(user => user.UserName != adminUser.UserName)
            .ToList();

        var userDTOs = new List<PersonalProfileDto>();
        foreach (var user in users) userDTOs.Add(await CreateUserDTOAsync(user));

        // todo: use a property for this
        ViewData["UserDTOs"] = userDTOs;

        return Page();
    }

    private async Task<bool> IsModeratorRole(ApplicationUser user)
    {
        var roles = await UserManager.GetRolesAsync(user);
        return roles.Contains(Roles.ModeratorRole);
    }

    private async Task<PersonalProfileDto> CreateUserDTOAsync(ApplicationUser user)
    {
        return new PersonalProfileDto
        {
            UserName = user.UserName,
            IsModerator = await IsModeratorRole(user),
            BlogCount = (uint)DbContext.Blog
                .Include(b => b.AppUser)
                .Where(blog => blog.AppUser.UserName == user.UserName)
                .ToList()
                .Count()
        };
    }

    public async Task<IActionResult> OnPostRemoveModeratorRoleAsync(string username)
    {
        var user = DbContext.Users.FirstOrDefault(user => user.UserName == username);
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
        var user = await DbContext.Users
            .SingleOrDefaultAsync(user => user.UserName == username);
        if (user == null)
        {
            Logger.LogError($"No user with ID {username} was found");
            return Page();
        }

        await UserManager.AddToRoleAsync(user, Roles.ModeratorRole);
        return RedirectToPage("Admin");
    }
}