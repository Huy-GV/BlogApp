using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BlogApp.Data;
using BlogApp.Data.DTOs;
using BlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using BlogApp.Pages;


namespace BlogApp.Pages.Blogs
{
    public class EditComment
    {
        public string Content { get; set; }
    }
    public class AddComment : EditComment
    {
        public int BlogID { get; set; }
    }
    [AllowAnonymous]
    public class ReadModel : BaseModel
    {
        [BindProperty]
        public AddComment CreateComment { get; set; }
        [BindProperty]
        public EditComment EditComment { get; set; }
        private readonly ILogger<ReadModel> _logger;
        public Blog Blog { get; set; }
        public ReadModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ReadModel> logger) : base(context, userManager)
        {
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) {
                return NotFound();
            }

            Blog = await Context.Blog
                .Include(blog => blog.Comments)
                .FirstOrDefaultAsync(blog => blog.ID == id);
            if (Blog == null)
                return NotFound();

            await IncrementViewCountAsync();

            if (User.Identity.IsAuthenticated)
            {
                var user = await UserManager.GetUserAsync(User);
                await CheckSuspensionExpiry(user.UserName);
                ViewData["IsSuspended"] = await SuspensionExists(user.UserName);
            } else
            {
                ViewData["IsSuspended"] = false;
            }


            ViewData["AuthorProfile"] = await GetSimpleProfileDTOAsync(Blog.Author);

            return Page();
        }
        private async Task<SimpleProfileDTO> GetSimpleProfileDTOAsync(string username)
        {
            var user = await UserManager.FindByNameAsync(Blog.Author);
            return new SimpleProfileDTO()
            {
                Username = Blog.Author,
                Description = Blog.Description,
                ProfilePath = user.ProfilePicture
            };
        }
        private async Task IncrementViewCountAsync()
        {
            Blog.ViewCount++;
            Context.Attach(Blog).State = EntityState.Modified;
            await Context.SaveChangesAsync();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!User.Identity.IsAuthenticated)
                return Challenge();

            if (!ModelState.IsValid)
            {
                _logger.LogError("Model state invalid when submitting comments");
                return NotFound();
            }

            var user = await UserManager.GetUserAsync(User);
            var username = user.UserName;

            var comment = new Comment
            {
                Author = user.UserName,
                Content = CreateComment.Content,
                Date = DateTime.Now,
                BlogID = CreateComment.BlogID
            };

            if (await SuspensionExists(username))
                return RedirectToPage("/Blogs/Read", new { id = comment.BlogID });

            Context.Add(comment);
            await Context.SaveChangesAsync();

            return RedirectToPage("/Blogs/Read", new { id = comment.BlogID });
        }
        public async Task<IActionResult> OnPostEditCommentAsync(int commentID)
        {
            if (!User.Identity.IsAuthenticated)
                return Challenge();

            if (!ModelState.IsValid)
            {
                _logger.LogError("Model state invalid when editting comments");
                return NotFound();
            }

            var user = await UserManager.GetUserAsync(User);
            var comment = await Context.Comment.FindAsync(commentID);
            if (user.UserName != comment.Author)
                return Forbid();

            comment.Content = EditComment.Content;
            Context.Attach(comment).State = EntityState.Modified;
            await Context.SaveChangesAsync();

            return RedirectToPage("/Blogs/Read", new { id = comment.BlogID });
        }
        public async Task<IActionResult> OnPostHideBlogAsync(int blogID)
        {
            if (!User.Identity.IsAuthenticated)
                return Challenge();

            var user = await UserManager.GetUserAsync(User);
            var roles = await UserManager.GetRolesAsync(user);
            
            if (!(roles.Contains(Roles.AdminRole) || roles.Contains(Roles.ModeratorRole))) 
                return Forbid();

            var blog = await Context.Blog.FindAsync(blogID);
            if (blog.Author == "admin")
                return Forbid();

            if (blog == null)
            {
                _logger.LogInformation("Blog not found error");
                return NotFound();
            }
            blog.IsHidden = true;
            blog.SuspensionExplanation = Messages.InappropriateBlog;
            await Context.SaveChangesAsync();
            return RedirectToPage("/Blogs/Read", new { id = blogID });
        }
        public async Task<IActionResult> OnPostHideCommentAsync(int commentID)
        {
            if (!User.Identity.IsAuthenticated)
                return Challenge();

            var user = await UserManager.GetUserAsync(User);
            var roles = await UserManager.GetRolesAsync(user);
            if (!(roles.Contains(Roles.AdminRole) || roles.Contains(Roles.ModeratorRole))) 
                return Forbid();

            var comment = await Context.Comment.FindAsync(commentID);
            if (comment.Author == "admin")
                return Forbid();
            if (comment == null)
            {
                _logger.LogInformation("Comment not found error");
                return NotFound();
            }
            comment.IsHidden = true;
            comment.SuspensionExplanation = Messages.InappropriateComment;
            await Context.SaveChangesAsync();
            return RedirectToPage("/Blogs/Read", new { id = comment.BlogID });
        }
        public async Task<IActionResult> OnPostDeleteBlogAsync(int blogID)
        {
            if (!User.Identity.IsAuthenticated)
                return Challenge();

            var user = await UserManager.GetUserAsync(User);
            var blog = await Context.Blog.FindAsync(blogID);

            if (user.UserName != blog.Author && !User.IsInRole(Roles.AdminRole))
                return Forbid();

            Context.Blog.Remove(blog);
            await Context.SaveChangesAsync();

            return RedirectToPage("/Blogs/Index");
        }
        public async Task<IActionResult> OnPostDeleteCommentAsync(int commentID)
        {
            if (!User.Identity.IsAuthenticated)
                return Challenge();
            
            var user = await UserManager.GetUserAsync(User);
            var comment = await Context.Comment.FindAsync(commentID);

            if (user.UserName != comment.Author && !User.IsInRole(Roles.AdminRole))
                return Forbid();

            Context.Comment.Remove(comment);
            await Context.SaveChangesAsync();

            return RedirectToPage("/Blogs/Read", new { id = comment.BlogID });
        }
    }
}
