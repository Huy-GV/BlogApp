using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BlogApp.Data;
using BlogApp.Models;
using Microsoft.AspNetCore.Identity;

namespace BlogApp.Pages.Blogs
{
    [AllowAnonymous]
    public class IndexModel : BaseModel
    {
        public IndexModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager) : base(context, authorizationService, userManager)
        {

        }
        [BindProperty]
        public InputBlog InputBlog { get; set; }

        public IList<Blog> Blog { get;set; }

        public async Task OnGetAsync()
        {
            var user = await UserManager.GetUserAsync(User);
            Blog = await Context
                .Blog
                .Include(blog => blog.Comments)
                .ToListAsync();
            
            ViewData["IsSuspended"] = Context.Suspension.Any(s => s.Username == user.UserName);
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Challenge();
            }
            var user = await UserManager.GetUserAsync(User);
            var username = user.UserName;
            await CheckSuspensionExpiry(username);
            if (SuspensionExists(username))
                return RedirectToPage("./Index");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ERROR");
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
