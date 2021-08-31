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
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Pages.Admin
{
    [Authorize(Roles = "admin")]
    public class DetailsModel : BaseModel
    {
        private readonly ILogger<AdminModel> _logger;
        public DetailsModel(ApplicationDbContext context,
                          UserManager<IdentityUser> userManager,
                          ILogger<AdminModel> logger) : base(context, userManager)
        {
            _logger = logger;
        }
        [BindProperty]
        public Suspension SuspensionTicket { get; set; }
        public async Task<IActionResult> OnGetAsync(string? username)
        {
            if (username == null)
                return NotFound();

            var user = await UserManager.FindByNameAsync(username);
            if (user == null)
            {
                _logger.LogInformation("User not found");
                return NotFound();
            }
            
            ViewData["UserDTO"] = GetUserDTO(username);
            ViewData["SuspendedBlogs"] = GetSuspendedBlogs(username);
            ViewData["SuspendedComments"] = GetSuspendedComments(username);
            ViewData["Suspension"] = GetSuspension(username);

            return Page();
        }
        private Suspension GetSuspension(string username) {
            return Context.Suspension.FirstOrDefault(s => s.Username == username);
        }
        private UserDTO GetUserDTO(string username) {
            return new UserDTO()
            {
                Username = username,
                BlogCount = Context.Blog
                    .Where(blog => blog.Author == username)
                    .ToList()
                    .Count,
                CommentCount = Context.Comment
                    .Where(comment => comment.Author == username)
                    .ToList()
                    .Count
            };
        }
        private List<Blog> GetSuspendedBlogs(string username) {
            return Context.Blog
                .Where(blog => blog.Author == username)
                .Where(blog => blog.IsHidden)
                .ToList();
        }
        private List<Comment> GetSuspendedComments(string username) {
            return Context.Comment
                .Where(comment => comment.Author == username)
                .Where(comment => comment.IsHidden)
                .ToList();
        }

        public async Task<IActionResult> OnPostSuspendUserAsync() 
        {
            if (!SuspensionExists(SuspensionTicket.Username)) {
                Context.Suspension.Add(SuspensionTicket);
                await Context.SaveChangesAsync();
            } else {
                _logger.LogInformation("User has already been suspended");
            }
            return RedirectToPage("Details", new { username = SuspensionTicket.Username });
        }

        public async Task<IActionResult> OnPostLiftSuspensionAsync(string username) 
        {
            if (SuspensionExists(username)) {
                var suspension = Context.Suspension.FirstOrDefault(s => s.Username == username);
                Context.Suspension.Remove(suspension);
                await Context.SaveChangesAsync();
            } else {
                _logger.LogInformation("User has no suspensions");
            }

            return RedirectToPage("Details", new { username });
        }
        public async Task<IActionResult> OnPostUnhidePostAsync(int postID, string type)
        {
            Post post;
            _logger.LogDebug("Post type is " + type);
            if (type == "comment")
            {
                post = await Context.Comment.FindAsync(postID);
            } else if (type == "blog")
            {
                post = await Context.Blog.FindAsync(postID);
            } else
            {
                return NotFound("Post type not found");
            }
            if (post == null)
            {
                return NotFound();
            }

            post.IsHidden = false;
            post.SuspensionExplanation = "";
            Context.Attach(post).State = EntityState.Modified;
            await Context.SaveChangesAsync();

            return RedirectToPage("Details", new { username = post.Author });
        }

    }
}
