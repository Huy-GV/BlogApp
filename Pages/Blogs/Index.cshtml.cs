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
            Blog = await Context.Blog.ToListAsync();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ERROR");
                return RedirectToPage("ERROR");
            }
            var user = await UserManager.GetUserAsync(User);
            var blog = new Blog
            {
                Title = InputBlog.Title,
                Content = InputBlog.Content,
                Date = DateTime.Now,
                Author = user.UserName
            };
            Console.WriteLine(blog.Title);
            Console.WriteLine(blog.Content);
            Console.WriteLine(blog.Date);
            Console.WriteLine(blog.Author);
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
