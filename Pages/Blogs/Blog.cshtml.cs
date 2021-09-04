using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BlogApp.Data;
using BlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using BlogApp.Pages;

namespace BlogApp.Pages.Blogs
{
    [AllowAnonymous]
    public class BlogModel : BaseModel
    {
        [BindProperty]
        public AddCommentForm InputComment { get; set; }
        [BindProperty]
        public ContentForm EditBlogForm { get; set; }
        [BindProperty]
        public ContentForm EditComment { get; set; }
        private ILogger<BlogModel> _logger;
        public Blog Blog { get; set; }

        public BlogModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<BlogModel> logger) : base(context, userManager)
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

            if (User.Identity.IsAuthenticated)
            {
                var user = await UserManager.GetUserAsync(User);
                await CheckSuspensionExpiry(user.UserName);
                ViewData["IsSuspended"] = SuspensionExists(user.UserName);
            } else
            {
                ViewData["IsSuspended"] = false;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!User.Identity.IsAuthenticated)
                return Challenge();

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ERROR");
                return Page();
            }

            var user = await UserManager.GetUserAsync(User);
            var username = user.UserName;

            var comment = new Comment
            {
                Author = user.UserName,
                Content = InputComment.Content,
                Date = DateTime.Now,
                BlogID = InputComment.BlogID
            };

            if (await SuspensionExists(username))
                return RedirectToPage("./Blog", new { id = comment.BlogID });

            Context.Add(comment);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Blog", new { id = comment.BlogID });
        }

        public async Task<IActionResult> OnPostEditBlogAsync(int blogID)
        {
            var user = await UserManager.GetUserAsync(User);
            var blog = await Context.Blog.FindAsync(blogID);

            if (EditBlogForm.Content == "")
                return RedirectToPage("./Blog", new { id = blogID });
            if (!User.Identity.IsAuthenticated)
                return Challenge();
            if (user.UserName != blog.Author)
                return Forbid();

            blog.Content = EditBlogForm.Content;
            Context.Attach(blog).State = EntityState.Modified;
            await Context.SaveChangesAsync();

            return RedirectToPage("./Blog", new { id = blogID });


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

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostDeleteCommentAsync(int commentID)
        {
            var user = await UserManager.GetUserAsync(User);
            var comment = await Context.Comment.FindAsync(commentID);

            if (user.UserName != comment.Author && !User.IsInRole(Roles.AdminRole))
                return Forbid();

            Context.Comment.Remove(comment);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Blog", new { id = comment.BlogID });
        }

        public async Task<IActionResult> OnPostEditCommentAsync(int commentID)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ERROR");
                return Page();
            }

            var user = await UserManager.GetUserAsync(User);
            var comment = await Context.Comment.FindAsync(commentID);
            if (user.UserName != comment.Author)
                return Forbid();

            comment.Content = EditComment.Content;
            Context.Attach(comment).State = EntityState.Modified;
            await Context.SaveChangesAsync();

            return RedirectToPage("./Blog", new { id = comment.BlogID });
        }

        public async Task<IActionResult> OnPostHideBlogAsync(int blogID)
        {
            var blog = await Context.Blog.FindAsync(blogID);
            if (blog == null)
            {
                _logger.LogInformation("Blog not found error");
                return Page();
            }
            blog.IsHidden = true;
            blog.SuspensionExplanation = Messages.InappropriateBlog;
            await Context.SaveChangesAsync();
            return RedirectToPage("./Blog", new { id = blogID });
        }

        public async Task<IActionResult> OnPostHideCommentAsync(int commentID)
        {
            var comment = await Context.Comment.FindAsync(commentID);
            if (comment == null)
            {
                _logger.LogInformation("Comment not found error");
                return Page();
            }
            comment.IsHidden = true;
            comment.SuspensionExplanation = Messages.InappropriateComment;
            await Context.SaveChangesAsync();
            return RedirectToPage("./Blog", new { id = comment.BlogID });
        }

    }
    public class AddCommentForm : ContentForm
    {
        public int BlogID { get; set; }
    }
    public class ContentForm
    {
        public string Content { get; set; }
    }

}
