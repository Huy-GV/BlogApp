using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogApp.Data;
using BlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging; 
namespace BlogApp.Pages.Blogs
{
    [Authorize]
    public class CreateBlogModel : BaseModel
    {

        [BindProperty]
        public InputBlog InputBlog { get; set; }
        private ILogger<CreateBlogModel> _logger;
        public CreateBlogModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<CreateBlogModel> logger) : base(context, userManager)
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
                Title = InputBlog.Title,
                Content = InputBlog.Content,
                Date = DateTime.Now,
                Author = user.UserName
            };

            Context.Blog.Add(blog);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
    public class InputBlog
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }
}

