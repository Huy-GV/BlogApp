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
using BlogApp.Services;
namespace BlogApp.Pages.Admin
{
    [Authorize(Roles = "admin")]
    public class DetailsModel : BaseModel
    {
        private readonly ILogger<AdminModel> _logger;
        private readonly UserSuspensionService _suspensionService;
        [BindProperty]
        public Suspension SuspensionTicket { get; set; }
        public DetailsModel(RazorBlogDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<AdminModel> logger,
            UserSuspensionService userSuspensionService
            ) : base(context, userManager)
        {
            _suspensionService = userSuspensionService;
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
            ViewData["Suspension"] = await _suspensionService.FindAsync(username);

            return Page();
        }
        private UserDTO GetUserDTO(string username) {
            return new UserDTO()
            {
                Username = username,
                BlogCount = DbContext.Blog
                    .Where(blog => blog.Author == username)
                    .ToList()
                    .Count,
                CommentCount = DbContext.Comment
                    .Where(comment => comment.Author == username)
                    .ToList()
                    .Count
            };
        }

        private async Task<List<Blog>> GetHiddenBlogs(string username)
        {
            return await DbContext.Blog
                   .Where(blog => blog.Author == username && blog.IsHidden)
                   .ToListAsync();
        }
        private async Task<List<Comment>> GetHiddenComments(string username)
        {
            return await DbContext.Comment
                .Where(comment => comment.Author == username && comment.IsHidden)
                .ToListAsync();
        }
        public async Task<IActionResult> OnPostSuspendUserAsync() 
        {
            if (!(await _suspensionService.ExistsAsync(SuspensionTicket.Username))) {
                DbContext.Suspension.Add(SuspensionTicket);
                await DbContext.SaveChangesAsync();
            } else {
                _logger.LogInformation("User has already been suspended");
            }
            return RedirectToPage("Details", new { username = SuspensionTicket.Username });
        }
        public async Task<IActionResult> OnPostLiftSuspensionAsync(string username) 
        {
            if (await _suspensionService.ExistsAsync(username)) {
                var suspension = DbContext.Suspension.FirstOrDefault(s => s.Username == username);
                await _suspensionService.RemoveAsync(suspension);
            } else {
                _logger.LogInformation("User has no suspensions");
            }

            return RedirectToPage("Details", new { username });
        }
        public async Task<IActionResult> OnPostUnhideBlogAsync(int blogID)
        {
            var blog = await DbContext.Blog.FindAsync(blogID);
            if (blog == null)
            {
                _logger.LogError("blog not found");
                return NotFound();
            }
            blog.IsHidden = false;
            blog.SuspensionExplanation = string.Empty;
            DbContext.Attach(blog).State = EntityState.Modified;
            await DbContext.SaveChangesAsync();

            return RedirectToPage("Details", new { username = blog.Author });
        }
        public async Task<IActionResult> OnPostUnhideCommentAsync(int commentID)
        {
            var comment = await DbContext.Comment.FindAsync(commentID);
            if (comment == null)
            {
                _logger.LogError("Comment not found");
                return NotFound();
            }

            comment.IsHidden = false;
            comment.SuspensionExplanation = string.Empty;
            DbContext.Attach(comment).State = EntityState.Modified;
            await DbContext.SaveChangesAsync();

            return RedirectToPage("Details", new { username = comment.Author });
        }
        public async Task<IActionResult> OnPostDeleteCommentAsync(int commentID)
        {
            var comment = await DbContext.Comment.FindAsync(commentID);
            if (comment == null)
            {
                _logger.LogError("Comment not found");
                return NotFound();
            }
            DbContext.Comment.Remove(comment);
            await DbContext.SaveChangesAsync();
            return RedirectToPage("Details", new { username = comment.Author });
        }
        public async Task<IActionResult> OnPostDeleteBlogAsync(int blogID)
        {
            var blog = await DbContext.Blog.FindAsync(blogID);
            if (blog == null)
            {
                _logger.LogError("blog not found");
                return NotFound();
            }
            DbContext.Blog.Remove(blog);
            await DbContext.SaveChangesAsync();
            return RedirectToPage("Details", new { username = blog.Author });
        }
    }
}
