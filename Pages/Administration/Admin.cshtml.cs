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

namespace BlogApp.Pages.Administration
{
    public class UserDTO
    { 
        public string Username { get; set; }
        public string ID { get; set; }
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
            var users = _userManager.Users.ToList();
            List<UserDTO> userDTOs = new();
            foreach( var user in users)
            {
                userDTOs.Add(new UserDTO()
                {
                    Username = user.UserName,
                    ID = user.Id
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
        public async Task<IActionResult> OnPostAddModeratorRoleAsync(int userID)
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
