using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogApp.Data;
using BlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace BlogApp.Pages.Blogs
{
    public class EditBlog : InputBlog
    {
        public int ID {get; set;}
        public string ImagePath {get; set;}
    }
    [Authorize]
    public class EditModel : BaseModel
    {
        [BindProperty]
        public EditBlog EditBlog { get; set; }
        private ILogger<CreateModel> _logger;
        public EditModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<CreateModel> logger) : base(context, userManager)
        {
            _logger = logger;
        }
        public async Task<IActionResult> OnGetAsync(int? blogID, string? username)
        {
            if (blogID == null || username == null)
            {
                return NotFound();
            }

            var user = await UserManager.GetUserAsync(User);
            if (user.UserName != username)
            {
                return Unauthorized();
            }
            _logger.LogInformation($"User {username} is editing the blog with ID {blogID}");
            var blog = await Context.Blog.FirstOrDefaultAsync(blog => blog.ID == blogID);
            
            EditBlog = new EditBlog
            { 
                ID = blog.ID,
                Title = blog.Title,
                ImagePath = blog.ImagePath,
                Content = blog.Content
            };

            return Page();
        }
        public async Task<IActionResult> OnPostEditBlogAsync()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state when editing blog");
                foreach(var error in ModelState.Values.SelectMany( e => e.Errors))
                {
                    _logger.LogError($"ERROR: {error}");
                }
            }

            var user = await UserManager.GetUserAsync(User);
            var blog = await Context.Blog.FindAsync(EditBlog.ID);

            if (blog == null)
                return NotFound();
            if (EditBlog.Content == "")
                return RedirectToPage("/Blogs/Read", new { id = blog.ID });
            if (user.UserName != blog.Author)
                return Forbid();

            blog.Content = EditBlog.Content;
            Context.Attach(blog).State = EntityState.Modified;
            await Context.SaveChangesAsync();

            return RedirectToPage("/Blogs/Read", new { id = blog.ID });
        }
    }
}

