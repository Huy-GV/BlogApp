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
        [BindProperty]
        public Suspension SuspensionTicket { get; set; }
        public DetailsModel(ApplicationDbContext context,
                          UserManager<ApplicationUser> userManager,
                          ILogger<AdminModel> logger) : base(context, userManager)
        {
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(string username)
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
            ViewData["HiddenBlogs"] = await GetHiddenBlogs(username);
            ViewData["HiddenComments"] = await GetHiddenComments(username);
            ViewData["Suspension"] = await GetSuspension(username);

            return Page();
        }
        private UserDTO GetUserDTO(string username) {
            //TODO: change to SingleOrDefault
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

        private async Task<List<Blog>> GetHiddenBlogs(string username)
        {
            return await Context.Blog
                   .Where(blog => blog.Author == username && blog.IsHidden)
                   .ToListAsync();
        }
        private async Task<List<Comment>> GetHiddenComments(string username)
        {
            return await Context.Comment
                .Where(comment => comment.Author == username && comment.IsHidden)
                .ToListAsync();
        }
        public async Task<IActionResult> OnPostSuspendUserAsync() 
        {
            if (!(await SuspensionExists(SuspensionTicket.Username))) {
                Context.Suspension.Add(SuspensionTicket);
                await Context.SaveChangesAsync();
            } else {
                _logger.LogInformation("User has already been suspended");
            }
            return RedirectToPage("Details", new { username = SuspensionTicket.Username });
        }
        public async Task<IActionResult> OnPostLiftSuspensionAsync(string username) 
        {
            if (await SuspensionExists(username)) {
                var suspension = Context.Suspension.FirstOrDefault(s => s.Username == username);
                Context.Suspension.Remove(suspension);
                await Context.SaveChangesAsync();
            } else {
                _logger.LogInformation("User has no suspensions");
            }

            return RedirectToPage("Details", new { username });
        }
        public async Task<IActionResult> OnPostUnhideBlogAsync(int blogID)
        {
            var blog = await Context.Blog.FindAsync(blogID);
            if (blog == null)
            {
                _logger.LogError("blog not found");
                return NotFound();
            }
            blog.IsHidden = false;
            blog.SuspensionExplanation = string.Empty;
            Context.Attach(blog).State = EntityState.Modified;
            await Context.SaveChangesAsync();

            return RedirectToPage("Details", new { username = blog.Author });
        }
        public async Task<IActionResult> OnPostUnhideCommentAsync(int commentID)
        {
            var comment = await Context.Comment.FindAsync(commentID);
            if (comment == null)
            {
                _logger.LogError("Comment not found");
                return NotFound();
            }

            comment.IsHidden = false;
            comment.SuspensionExplanation = string.Empty;
            Context.Attach(comment).State = EntityState.Modified;
            await Context.SaveChangesAsync();

            return RedirectToPage("Details", new { username = comment.Author });
        }
        public async Task<IActionResult> OnPostDeleteCommentAsync(int commentID)
        {
            var comment = await Context.Comment.FindAsync(commentID);
            if (comment == null)
            {
                _logger.LogError("Comment not found");
                return NotFound();
            }
            Context.Comment.Remove(comment);
            await Context.SaveChangesAsync();
            return RedirectToPage("Details", new { username = comment.Author });
        }
        public async Task<IActionResult> OnPostDeleteBlogAsync(int blogID)
        {
            var blog = await Context.Blog.FindAsync(blogID);
            if (blog == null)
            {
                _logger.LogError("blog not found");
                return NotFound();
            }
            Context.Blog.Remove(blog);
            await Context.SaveChangesAsync();
            return RedirectToPage("Details", new { username = blog.Author });
        }
    }
}
