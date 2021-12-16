using BlogApp.Data;
using BlogApp.Data.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogApp.Data.DTOs;
using BlogApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Pages.Admin
{
    //PASSWORD: Admin123@@
    [Authorize(Roles = "admin")]
    public class AdminModel : BaseModel<AdminModel>
    {
        public AdminModel(RazorBlogDbContext context,
                          UserManager<ApplicationUser> userManager,
                          ILogger<AdminModel> logger) : base(context, userManager, logger)
        {
            
        }
        
        //TODO: add a filter that shows moderators only
        public async Task<IActionResult> OnGetAsync()
        {
            var users = UserManager.Users
                .AsNoTracking()
                .ToList()
                .Where(user => user.UserName != "admin");
            
            List<UserDTO> userDTOs = new();
            foreach( var user in users)
            {   
                userDTOs.Add(await CreateUserDTOAsync(user));
            }

            ViewData["UserDTOs"] = userDTOs;

            return Page();
        }
        private async Task<bool> IsModeratorRole(ApplicationUser user)
        {
            var roles = await UserManager.GetRolesAsync(user);
            return roles.Contains(Roles.ModeratorRole);
        }
        private async Task<UserDTO> CreateUserDTOAsync(ApplicationUser user)
        {
            return new UserDTO
            {
                Username = user.UserName,
                IsModerator = await IsModeratorRole(user),
                BlogCount = DbContext.Blog
                .Where(blog => blog.Author == user.UserName)
                .ToList()
                .Count
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
}
