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
using BlogApp.Services;
using BlogApp.Data.FormModels;
namespace BlogApp.Pages.Blogs
{

    [Authorize]
    public class EditModel : BaseModel
    {
        [BindProperty]
        public EditBlog EditBlog { get; set; }
        private readonly ILogger<CreateModel> _logger;
        private readonly ImageFileService _imageFileService;
        public EditModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<CreateModel> logger,
            ImageFileService imageFileService) : base(context, userManager)
        {
            _logger = logger;
            _imageFileService = imageFileService;
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
                Content = blog.Content,
                Description = blog.Description
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
            blog.Title = EditBlog.Title;
            blog.Description = EditBlog.Description;

            if (EditBlog.CoverImage != null)
            {
                _imageFileService.DeleteImage(blog.ImagePath);
                blog.ImagePath = await _imageFileService.UploadBlogImageAsync(EditBlog.CoverImage);
            }
            
            Context.Attach(blog).State = EntityState.Modified;
            await Context.SaveChangesAsync();

            return RedirectToPage("/Blogs/Read", new { id = blog.ID });
        }
    }
}

