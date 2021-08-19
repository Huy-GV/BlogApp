using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlogApp.Authorization;
using BlogApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BlogApp.Areas.Identity.Pages.Account;
using Microsoft.Extensions.Logging;

namespace BlogApp.Pages.Admin
{
    //PASSWORD: Admin123@@
    public class UserDTO
    { 
        public string Username { get; set; }
        public string ID { get; set; }
        public string JoinDate { get; set; }
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
        public IActionResult OnGetAsync()
        {
            //TODO: IMPLEMENT JOIN DATES FOR USERS, WRITE A CUSTOM USER IDENTITY CLASS?
            var users = _userManager.Users.ToList().Where(user => user.UserName != "admin");
            List<UserDTO> userDTOs = new();
            foreach( var user in users)
            {   
                userDTOs.Add(new UserDTO()
                {
                    Username = user.UserName,
                    ID = user.Id,
                    PostCount = _context.Blog
                    .Where(blog => blog.Author == user.UserName)
                    .ToList()
                    .Count
                });
            }
            ViewData["UserDTOs"] = userDTOs;
            return Page();
        }

        public async Task<IActionResult> OnPostRemoveModeratorRoleAsync(int userID)
        {
            var user = await _context.Users.FindAsync(userID);
            if (user == null)
            {
                _logger.LogError($"No user with ID {userID} was found");
                return Page();
            }
            await _userManager.RemoveFromRoleAsync(user, Roles.ModeratorRole);

            return Page();
        }
        public async Task<IActionResult> OnPostAssignModeratorRoleAsync(int userID)
        {
            var user = await _context.Users.FindAsync(userID);
            if (user == null)
            {
                _logger.LogError($"No user with ID {userID} was found");
                return Page();
            }

            await _userManager.AddToRoleAsync(user, Roles.ModeratorRole);
            return Page();
        }

    }
}
