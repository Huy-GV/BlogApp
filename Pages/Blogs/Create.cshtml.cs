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
using Microsoft.AspNetCore.Http;
using BlogApp.Services;
using System.ComponentModel.DataAnnotations;
namespace BlogApp.Pages.Blogs
{
    public class InputBlog
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Description { get; set; }
        [Display(Name = "Cover image")]
        public IFormFile CoverImage { get; set; }
    }
    [Authorize]
    public class CreateModel : BaseModel
    {
        [BindProperty]
        public InputBlog CreateBlog { get; set; }
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

