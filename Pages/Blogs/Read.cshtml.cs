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
using System.Linq;
using BlogApp.Data.ViewModels;

namespace BlogApp.Pages.Blogs
{
    [AllowAnonymous]
    public class ReadModel : BasePageModel<ReadModel>
    {
        [BindProperty]
        public CommentViewModel CreateCommentViewModel { get; set; }

        [BindProperty]
        public CommentViewModel EditCommentViewModel { get; set; }

        private readonly UserModerationService _moderationService;
        public Blog Blog { get; set; }
        public DetailedBlogDto DetailedBlogDto { get; set; }

        public ReadModel(
            RazorBlogDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ReadModel> logger,
            UserModerationService moderationService) : base(
                context, userManager, logger)
        {
            _moderationService = moderationService;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await DbContext.Blog
                .Include(blog => blog.AppUser)
                .Include(blog => blog.Comments)
                .ThenInclude(comment => comment.AppUser)
                .SingleOrDefaultAsync(blog => blog.Id == id);

            if (blog == null)
            {
                return NotFound();
            }

            var blogAuthor = new
            {
                UserName = blog.AppUser?.UserName ?? "Deleted User",
                // todo: un-hardcode default profile pic
                ProfileImageUri = blog.AppUser?.ProfileImageUri ?? "default.jpg",
                Description = blog.AppUser?.Description ?? "Deleted User"
            };

            DbContext.Blog.Update(blog);
            blog.ViewCount++;
            await DbContext.SaveChangesAsync();

            // todo: refactor this
            ViewData["IsSuspended"] = false;
            if (User.Identity.IsAuthenticated)
            {
                ViewData["IsSuspended"] = await _moderationService.BanTicketExistsAsync(User.Identity.Name);
            }

            DetailedBlogDto = new DetailedBlogDto
            {
                Id = blog.Id,
                Introduction = blog.Introduction,
                Content = blog.Content,
                CoverImageUri = blog.CoverImageUri,
                Date = blog.Date,
                IsHidden = blog.IsHidden,
                AuthorDescription = blogAuthor.Description,
                AuthorName = blogAuthor.UserName,
                AuthorProfileImageUri = blogAuthor.ProfileImageUri,
                CommentDtos = blog.Comments
                    .Select(c => new CommentDto
                    {
                        Id = c.Id,
                        Date = c.Date,
                        Content = c.Content,
                        AuthorName = c.AppUser?.UserName ?? "Deleted User",
                        AuthorProfileImageUri = c.AppUser?.ProfileImageUri ?? "default.jpg",
                        IsHidden = c.IsHidden,
                    })
                    .ToList()
            };

            return Page();
        }

        public async Task<IActionResult> OnPostCreateCommentAsync(int blogId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                Logger.LogError("Model state invalid when submitting new comment.");
                //TODO: return an appropriate response
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

            if (await _moderationService.BanTicketExistsAsync(username))
                return RedirectToPage("/Blogs/Read", new { id = blogId });

            var entry = DbContext.Comment.Add(new Comment
            {
                Date = DateTime.Now,
                AppUserId = user.Id
            });

            entry.CurrentValues.SetValues(CreateCommentViewModel);
            await DbContext.SaveChangesAsync();

            return RedirectToPage("/Blogs/Read", new { id = blogId });
        }

        public async Task<IActionResult> OnPostEditCommentAsync(int commentID)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                Logger.LogError("Invalid model state when editting comments");
                return NotFound();
            }

            var user = await UserManager.GetUserAsync(User);
            var comment = await DbContext.Comment
                .Include(x => x.AppUser)
                .SingleOrDefaultAsync(x => x.Id == commentID);

            if (user.UserName != comment.AppUser.UserName)
            {
                return Forbid();
            }

            DbContext.Comment.Update(comment).CurrentValues.SetValues(EditCommentViewModel);
            await DbContext.SaveChangesAsync();

            return RedirectToPage("/Blogs/Read", new { id = comment.BlogID });
        }

        public async Task<IActionResult> OnPostHideBlogAsync(int blogID)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Challenge();
            }

            var user = await UserManager.GetUserAsync(User);
            var roles = await UserManager.GetRolesAsync(user);

            if (!roles.Contains(Roles.AdminRole) && !roles.Contains(Roles.ModeratorRole))
            {
                return Forbid();
            }

            var blog = await DbContext.Blog
                .Include(x => x.AppUser)
                .SingleOrDefaultAsync(x => x.Id == blogID);

            if (blog == null)
            {
                return NotFound();
            }

            if (await UserManager.IsInRoleAsync(blog.AppUser, Roles.AdminRole))
            {
                return Forbid();
            }

            // blog.SuspensionExplanation = Messages.InappropriateBlog;
            // await DbContext.SaveChangesAsync();

            await _moderationService.HideBlogAsync(blogID);
            return RedirectToPage("/Blogs/Read", new { id = blogID });
        }

        public async Task<IActionResult> OnPostHideCommentAsync(int commentID)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Challenge();
            }

            var user = await UserManager.GetUserAsync(User);
            var roles = await UserManager.GetRolesAsync(user);
            if (!roles.Contains(Roles.AdminRole) && !roles.Contains(Roles.ModeratorRole))
            {
                return Forbid();
            }

            var comment = await DbContext.Comment
                .Include(x => x.AppUser)
                .SingleOrDefaultAsync(x => x.Id == commentID);

            if (comment == null)
            {
                return NotFound();
            }

            if (await UserManager.IsInRoleAsync(comment.AppUser, Roles.AdminRole))
            {
                return Forbid();
            }

            // comment.SuspensionExplanation = Messages.InappropriateComment;
            // await DbContext.SaveChangesAsync();

            await _moderationService.HideCommentAsync(commentID);
            return RedirectToPage("/Blogs/Read", new { id = comment.BlogID });
        }

        public async Task<IActionResult> OnPostDeleteBlogAsync(int blogId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Challenge();
            }

            var blog = await DbContext.Blog
                .Include(x => x.AppUser)
                .SingleOrDefaultAsync(x => x.Id == blogId);

            // todo: check identity name and user.UserName
            if (User.Identity.Name != blog.AppUser.UserName)
            {
                return Forbid();
            }

            if (blog == null)
            {
                return NotFound();
            }

            DbContext.Blog.Remove(blog);
            await DbContext.SaveChangesAsync();

            return RedirectToPage("/Blogs/Index");
        }

        public async Task<IActionResult> OnPostDeleteCommentAsync(int commentId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Challenge();
            }

            var comment = await DbContext.Comment
                .Include(x => x.AppUser)
                .SingleOrDefaultAsync(x => x.Id == commentId);

            if (comment == null)
            {
                return NotFound();
            }

            if (User.Identity.Name != comment.AppUser.UserName)
            {
                return Forbid();
            }

            DbContext.Comment.Remove(comment);
            await DbContext.SaveChangesAsync();

            return RedirectToPage("/Blogs/Read", new { id = comment.BlogID });
        }
    }
}