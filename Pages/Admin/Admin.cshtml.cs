using BlogApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogApp.Data.DTOs;
using BlogApp.Pages;

namespace BlogApp.Pages.Admin
{
    //PASSWORD: Admin123@@
    [Authorize(Roles = "admin")]
    public class AdminModel : BaseModel
    {
        private ILogger<AdminModel> _logger;
        public AdminModel(ApplicationDbContext context,
                          UserManager<IdentityUser> userManager,
                          ILogger<AdminModel> logger) : base(context, userManager)
        {
            _logger = logger;
        }
        
        //TODO: add a filter that shows moderators only
        public async Task<IActionResult> OnGetAsync()
        {
            var users = UserManager.Users
                .ToList()
                .Where(user => user.UserName != "admin");
            List<UserDTO> userDTOs = new();
            foreach( var user in users)
            {   
                userDTOs.Add(new UserDTO()
                {
                    Username = user.UserName,
                    IsModerator = await IsModeratorRole(user),
                    BlogCount = Context.Blog
                    .Where(blog => blog.Author == user.UserName)
                    .ToList()
                    .Count
                });
            }
            ViewData["UserDTOs"] = userDTOs;
            return Page();
        }
        private async Task<bool> IsModeratorRole(IdentityUser user)
        {
            var roles = await UserManager.GetRolesAsync(user);
            return roles.Contains(Roles.ModeratorRole);
        }

        public async Task<IActionResult> OnPostRemoveModeratorRoleAsync(string username)
        {
            var user = Context.Users.FirstOrDefault(user => user.UserName == username);
            if (user == null)
            {
                _logger.LogError($"No user with ID {username} was found");
                return Page();
            }
            await UserManager.RemoveFromRoleAsync(user, Roles.ModeratorRole);

            return RedirectToPage("Admin");
        }
        public async Task<IActionResult> OnPostAssignModeratorRoleAsync(string username)
        {
            var user = Context.Users.FirstOrDefault(user => user.UserName == username);
            if (user == null)
            {
                _logger.LogError($"No user with ID {username} was found");
                return Page();
            }

            await UserManager.AddToRoleAsync(user, Roles.ModeratorRole);
            return RedirectToPage("Admin");
        }

    }
}
