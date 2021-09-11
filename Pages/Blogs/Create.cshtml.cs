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
namespace BlogApp.Pages.Blogs
{
    public class InputBlog
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }
    [Authorize]
    public class CreateModel : BaseModel
    {
        [BindProperty]
        public InputBlog CreateBlog { get; set; }
        private ILogger<CreateModel> _logger;
        public CreateModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<CreateModel> logger) : base(context, userManager)
        {
            _logger = logger;
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
                Content = CreateBlog.Content,
                Date = DateTime.Now,
                Author = user.UserName
            };

            Context.Blog.Add(blog);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }

}

