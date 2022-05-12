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
    public class DetailsModel : BasePageModel<DetailsModel>
    {
        private readonly UserModerationService _suspensionService;

        [BindProperty]
        public BanTicket SuspensionTicket { get; set; }

        public DetailsModel(RazorBlogDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<DetailsModel> logger,
            UserModerationService userSuspensionService
            ) : base(context, userManager, logger)
        {
            _suspensionService = userSuspensionService;
        }

        public async Task<IActionResult> OnGetAsync(string? username)
        {
            if (username == null)
                return NotFound();

            var user = await UserManager.FindByNameAsync(username);
            if (user == null)
            {
                Logger.LogInformation("User not found");
                return NotFound();
            }

            ViewData["UserDTO"] = GetUserDTO(username);
            ViewData["HiddenBlogs"] = await GetHiddenBlogs(username);
            ViewData["HiddenComments"] = await GetHiddenComments(username);
            ViewData["Suspension"] = await _suspensionService.FindAsync(username);

            return Page();
        }

        private PersonalProfileDto GetUserDTO(string username)
        {
            return new PersonalProfileDto()
            {
                UserName = username,
                BlogCount = (uint)DbContext.Blog
                    .Include(b => b.AppUser)
                    .Where(blog => blog.AppUser.UserName == username)
                    .ToList()
                    .Count,
                CommentCount = (uint)DbContext.Comment
                    .Include(c => c.AppUser)
                    .Where(c => c.AppUser.UserName == username)
                    .ToList()
                    .Count
            };
        }

        private async Task<List<Blog>> GetHiddenBlogs(string username)
        {
            // todo: get dto here
            return await DbContext.Blog
                   .Include(b => b.AppUser)
                   .Where(b => b.AppUser.UserName == username && b.IsHidden)
                   .ToListAsync();
        }

        private async Task<List<Comment>> GetHiddenComments(string username)
        {
            // todo: get dto here
            return await DbContext.Comment
                   .Include(c => c.AppUser)
                   .Where(c => c.AppUser.UserName == username && c.IsHidden)
                   .ToListAsync();
        }

        public async Task<IActionResult> OnPostSuspendUserAsync()
        {
            if (!(await _suspensionService.BanTicketExistsAsync(SuspensionTicket.UserName)))
            {
                DbContext.BanTicket.Add(SuspensionTicket);
                await DbContext.SaveChangesAsync();
            }
            else
            {
                Logger.LogInformation("User has already been suspended");
            }
            return RedirectToPage("Details", new { username = SuspensionTicket.UserName });
        }

        // todo: rename and use moderation service
        public async Task<IActionResult> OnPostLiftSuspensionAsync(string username)
        {
            if (await _suspensionService.BanTicketExistsAsync(username))
            {
                var suspension = await DbContext.BanTicket
                    .SingleOrDefaultAsync(s => s.UserName == username);
                await _suspensionService.RemoveAsync(suspension);
            }
            else
            {
                Logger.LogInformation("User has no suspensions");
            }

            return RedirectToPage("Details", new { username });
        }

        public async Task<IActionResult> OnPostUnhideBlogAsync(int blogID)
        {
            var blog = await DbContext.Blog.FindAsync(blogID);
            if (blog == null)
            {
                Logger.LogError("blog not found");
                return NotFound();
            }

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
                Logger.LogError("Comment not found");
                return NotFound();
            }

            // todo: add content moderation service?
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
                Logger.LogError("Comment not found");
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
                Logger.LogError("blog not found");
                return NotFound();
            }
            DbContext.Blog.Remove(blog);
            await DbContext.SaveChangesAsync();
            return RedirectToPage("Details", new { username = blog.Author });
        }
    }
}