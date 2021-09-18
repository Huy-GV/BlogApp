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
        private enum PostType {
            Blog,
            Comment
        }
        private readonly ILogger<AdminModel> _logger;
        [BindProperty]
        public Suspension SuspensionTicket { get; set; }
        public DetailsModel(ApplicationDbContext context,
                          UserManager<IdentityUser> userManager,
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
            ViewData["SuspendedBlogs"] = await GetSuspendedPosts<Blog>(username);
            ViewData["SuspendedComments"] = await GetSuspendedPosts<Comment>(username);
            ViewData["Suspension"] = await GetSuspension(username);

            return Page();
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
        private async Task<IList<T>> GetSuspendedPosts<T>(string username) where T : Post
        {
            if (typeof(T) == typeof(Comment))
            {
                return await Context.Comment
                    .Where(comment => comment.Author == username)
                    .Where(comment => comment.IsHidden)
                    .ToListAsync() as IList<T>;
            } else if (typeof(T) == typeof(Blog))
            {
                return await Context.Blog
                    .Where(blog => blog.Author == username)
                    .Where(blog => blog.IsHidden)
                    .ToListAsync() as IList<T>;
            } else
            {
                throw new Exception("Unknown type for suspended post");
            }
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
        //TODO: separate methods again because the length are similar
        public async Task<IActionResult> OnPostDeletePostAsync(int postID, string type)
        {
            string username;
            if (type == "comment")
            {
                var comment = await Context.Comment.FindAsync(postID);
                if (comment == null)
                {
                    _logger.LogError("Comment not found");
                    return NotFound();
                }
                username = comment.Author;
                Context.Comment.Remove(comment);
            } else if (type == "blog")
            {
                var blog = await Context.Blog.FindAsync(postID);
                if (blog == null)
                {
                    _logger.LogError("Blog not found");
                    return NotFound();
                }
                username = blog.Author;
                Context.Blog.Remove(blog);
            } else 
            {
                _logger.LogError("Unable to delete post with unknown type");
                return NotFound();
            }
            await Context.SaveChangesAsync();
            return RedirectToPage("Details", new { username });
        }
    }
}
