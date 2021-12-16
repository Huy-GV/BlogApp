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
using BlogApp.Data.ViewModel;
namespace BlogApp.Pages.Blogs
{

    [Authorize]
    public class EditModel : BaseModel<EditModel>
    {
        [BindProperty]
        public EditBlogViewModel EditBlogVM { get; set; }
        private readonly ImageFileService _imageFileService;
        public EditModel(
            RazorBlogDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<EditModel> logger,
            ImageFileService imageFileService) : base(context, userManager, logger)
        {
            _imageFileService = imageFileService;
        }
        public async Task<IActionResult> OnGetAsync(int? blogID, string? username)
        {
            if (blogID == null || username == null)
                return NotFound();

            if (User.Identity.Name != username)
            {
                return Unauthorized();
            }

            var blog = await DbContext.Blog.FindAsync(blogID);
            
            EditBlogVM = new EditBlogViewModel
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
                Logger.LogError("Invalid model state when editing blog");
                return Page();
            }

            var user = await UserManager.GetUserAsync(User);
            var blog = await DbContext.Blog.FindAsync(EditBlogVM.ID);

            if (blog == null)
                return NotFound();
            if (EditBlogVM.Content == "")
                return RedirectToPage("/Blogs/Read", new { id = blog.ID });
            if (user.UserName != blog.Author)
                return Forbid();

            DbContext.Blog.Attach(blog).CurrentValues.SetValues(EditBlogVM);

            if (EditBlogVM.CoverImage != null)
            {
                _imageFileService.DeleteImage(blog.ImagePath);
                blog.ImagePath = await _imageFileService
                .UploadBlogImageAsync(EditBlogVM.CoverImage);
            }

            DbContext.Attach(blog).State = EntityState.Modified;
            await DbContext.SaveChangesAsync();

            return RedirectToPage("/Blogs/Read", new { id = blog.ID });
        }
    }
}

