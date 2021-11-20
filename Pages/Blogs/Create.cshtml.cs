using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogApp.Data;
using BlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; 
using BlogApp.Services;
using BlogApp.Data.FormModels;
namespace BlogApp.Pages.Blogs
{
    [Authorize]
    public class CreateModel : BaseModel
    {
        [BindProperty]
        public CreateBlog CreateBlog { get; set; }
        private readonly ILogger<CreateModel> _logger;
        private readonly ImageFileService _imageFileService;
        public CreateModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<CreateModel> logger,
            ImageFileService imageFileService) : base(context, userManager)
        {
            _logger = logger;
            _imageFileService = imageFileService;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await UserManager.GetUserAsync(User);
            var username = user.UserName;

            if (await SuspensionExists(username))
                return RedirectToPage("./Index");

            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await UserManager.GetUserAsync(User);
            var username = user.UserName;

            if (await SuspensionExists(username))
                return RedirectToPage("./Index");

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state when submitting new post");
                return Page();
            }
            var blog = new Blog
            {
                Title = CreateBlog.Title,
                Description = CreateBlog.Description,
                Content = CreateBlog.Content,
                ImagePath = await _imageFileService.UploadBlogImageAsync(CreateBlog.CoverImage),
                Date = DateTime.Now,
                Author = user.UserName
            };

            Context.Blog.Add(blog);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }

}

