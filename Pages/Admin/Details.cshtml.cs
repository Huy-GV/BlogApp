using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlogApp.Pages;
using BlogApp.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BlogApp.Data.DTOs;
using BlogApp.Models;

namespace BlogApp.Pages.Admin
{
    public class DetailsModel : PageModel
    {
        private ApplicationDbContext _context { get; }
        private IAuthorizationService _authorizationService { get; }
        private UserManager<IdentityUser> _userManager { get; }
        private readonly ILogger<AdminModel> _logger;
        public DetailsModel(ApplicationDbContext context,
                          IAuthorizationService authorizationService,
                          UserManager<IdentityUser> userManager,
                          ILogger<AdminModel> logger)
        {
            _context = context;
            _authorizationService = authorizationService;
            _userManager = userManager;
            _logger = logger;
        }
        public UserDTO UserDTO { get; set; }
        [BindProperty]
        public Suspension SuspensionTicket { get; set; }
        public async Task<IActionResult> OnGetAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                _logger.LogInformation("User not found");
                return NotFound();
            }
            
            ViewData["UserDTO"] = GetUserDTO(username);
            ViewData["SuspendedBlogs"] = GetSuspendedBlogs(username);
            ViewData["SuspendedComments"] = GetSuspendedComments(username);

            return Page();
        }
        private UserDTO GetUserDTO(string username) {
            return new UserDTO()
            {
                Username = username,
                BlogCount = _context.Blog
                    .Where(blog => blog.Author == username)
                    .ToList()
                    .Count,
                CommentCount = _context.Comment
                    .Where(comment => comment.Author == username)
                    .ToList()
                    .Count
            };
        }
        private List<Blog> GetSuspendedBlogs(string username) {
            return _context.Blog
                .Where(blog => blog.Author == username)
                .Where(blog => blog.IsHidden)
                .ToList();
        }
        private List<Comment> GetSuspendedComments(string username) {
            return _context.Comment
                .Where(comment => comment.Author == username)
                .Where(comment => comment.IsHidden)
                .ToList();
        }

        public async Task<IActionResult> OnPostSuspendUserAsync() 
        {
            _context.Add(SuspensionTicket);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Details", new { username = SuspensionTicket.Username });
        }
        public async Task<IActionResult> OnPostLiftSuspensionAsync() 
        {
            _context.Remove(SuspensionTicket);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Details", new { username = SuspensionTicket.Username });
        }
    }
}
