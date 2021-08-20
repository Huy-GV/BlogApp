using BlogApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogApp.Pages.Admin
{
    //PASSWORD: Admin123@@
    public class UserDTO
    { 
        public string Username { get; set; }
        public string JoinDate { get; set; }
        public bool IsModerator { get; set; } = false;
        public int PostCount { get; set; }
    }
    //TODO: create a custom handler?
    [Authorize(Roles = "admin")]
    public class AdminModel : PageModel
    {
        private ApplicationDbContext _context { get; }
        private IAuthorizationService _authorizationService { get; }
        private UserManager<IdentityUser> _userManager { get; }
        private ILogger<AdminModel> _logger;
        public AdminModel(ApplicationDbContext context,
                          IAuthorizationService  authorizationService,
                          UserManager<IdentityUser> userManager,
                          ILogger<AdminModel> logger)
        {
            _context = context;
            _authorizationService = authorizationService;
            _userManager = userManager;
            _logger = logger;
        }
        
        //TODO: add a filter that shows moderators only
        public async Task<IActionResult> OnGetAsync()
        {
            //TODO: IMPLEMENT JOIN DATES FOR USERS, WRITE A CUSTOM USER IDENTITY CLASS?
            var users = _userManager.Users
                .ToList()
                .Where(user => user.UserName != "admin");
            List<UserDTO> userDTOs = new();
            foreach( var user in users)
            {   
                userDTOs.Add(new UserDTO()
                {
                    Username = user.UserName,
                    IsModerator = await IsModeratorRole(user),
                    PostCount = _context.Blog
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
            var roles = await _userManager.GetRolesAsync(user);
            return roles.Contains(Roles.ModeratorRole);
        }

        public async Task<IActionResult> OnPostRemoveModeratorRoleAsync(string username)
        {
            var user = _context.Users.FirstOrDefault(user => user.UserName == username);
            if (user == null)
            {
                _logger.LogError($"No user with ID {username} was found");
                return Page();
            }
            await _userManager.RemoveFromRoleAsync(user, Roles.ModeratorRole);

            return RedirectToPage("Admin");

        }
        public async Task<IActionResult> OnPostAssignModeratorRoleAsync(string username)
        {
            var user = _context.Users.FirstOrDefault(user => user.UserName == username);
            if (user == null)
            {
                _logger.LogError($"No user with ID {username} was found");
                return Page();
            }

            await _userManager.AddToRoleAsync(user, Roles.ModeratorRole);
            return RedirectToPage("Admin");
        }

    }
}
