using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using BlogApp.Data;
using BlogApp.Data.DTOs;
using BlogApp.Data.Constants;
using BlogApp.Data.ViewModel;
using BlogApp.Services;
namespace BlogApp.Pages.Blogs
{

    [AllowAnonymous]
    public class ReadModel : BaseModel
    {
        [BindProperty]
        public CreateCommentViewModel CreateCommentVM { get; set; }
        [BindProperty]
        public EditCommentViewModel EditCommentVM { get; set; }
        private readonly ILogger<ReadModel> _logger;
        private readonly UserSuspensionService _suspensionService;
        public Blog Blog { get; set; }
        public ReadModel(
            RazorBlogDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ReadModel> logger,
            UserSuspensionService suspensionService) : base(context, userManager)
        {
            _logger = logger;
            _suspensionService = suspensionService;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) {
                return NotFound();
            }

            Blog = await DbContext.Blog
                .Include(blog => blog.Comments)
                .FirstOrDefaultAsync(blog => blog.ID == id);
            if (Blog == null)
                return NotFound();

            await IncrementViewCountAsync();

            if (User.Identity.IsAuthenticated)
            {
                var user = await UserManager.GetUserAsync(User);
                ViewData["IsSuspended"] = await _suspensionService.ExistsAsync(user.UserName);
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
            DbContext.Attach(Blog).State = EntityState.Modified;
            await DbContext.SaveChangesAsync();
        }
        public async Task<IActionResult> OnPostCreateCommentAsync()
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

            // var comment = new Comment
            // {
            //     Author = user.UserName,
            //     Content = CreateCommentVM.Content,
            //     Date = DateTime.Now,
            //     BlogID = CreateCommentVM.BlogID
            // };

            if (await _suspensionService.ExistsAsync(username))
                return RedirectToPage("/Blogs/Read", new { id = CreateCommentVM.BlogID });

            var entry = DbContext.Comment.Add(new Comment
            {
                Author = user.UserName,
                Date = DateTime.Now
            });

            entry.CurrentValues.SetValues(CreateCommentVM);
            await DbContext.SaveChangesAsync();

            return RedirectToPage("/Blogs/Read", new { id = CreateCommentVM.BlogID });
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
            var comment = await DbContext.Comment.FindAsync(commentID);
            if (user.UserName != comment.Author)
                return Forbid();

            comment.Content = EditCommentVM.Content;
            DbContext.Attach(comment).State = EntityState.Modified;
            await DbContext.SaveChangesAsync();

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

            var blog = await DbContext.Blog.FindAsync(blogID);
            if (blog.Author == "admin")
                return Forbid();

            if (blog == null)
            {
                _logger.LogInformation("Blog not found error");
                return NotFound();
            }
            blog.IsHidden = true;
            blog.SuspensionExplanation = Messages.InappropriateBlog;
            await DbContext.SaveChangesAsync();
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

            var comment = await DbContext.Comment.FindAsync(commentID);
            if (comment.Author == "admin")
                return Forbid();
            if (comment == null)
            {
                _logger.LogInformation("Comment not found error");
                return NotFound();
            }
            comment.IsHidden = true;
            comment.SuspensionExplanation = Messages.InappropriateComment;
            await DbContext.SaveChangesAsync();
            return RedirectToPage("/Blogs/Read", new { id = comment.BlogID });
        }
        public async Task<IActionResult> OnPostDeleteBlogAsync(int blogID)
        {
            if (!User.Identity.IsAuthenticated)
                return Challenge();

            var user = await UserManager.GetUserAsync(User);
            var blog = await DbContext.Blog.FindAsync(blogID);

            if (user.UserName != blog.Author && !User.IsInRole(Roles.AdminRole))
                return Forbid();

            DbContext.Blog.Remove(blog);
            await DbContext.SaveChangesAsync();

            return RedirectToPage("/Blogs/Index");
        }
        public async Task<IActionResult> OnPostDeleteCommentAsync(int commentID)
        {
            if (!User.Identity.IsAuthenticated)
                return Challenge();
            
            var user = await UserManager.GetUserAsync(User);
            var comment = await DbContext.Comment.FindAsync(commentID);

            if (user.UserName != comment.Author && !User.IsInRole(Roles.AdminRole))
                return Forbid();

            DbContext.Comment.Remove(comment);
            await DbContext.SaveChangesAsync();

            return RedirectToPage("/Blogs/Read", new { id = comment.BlogID });
        }
    }
}
