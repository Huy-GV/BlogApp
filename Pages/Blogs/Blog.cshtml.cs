using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BlogApp.Data;
using BlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;


namespace BlogApp.Pages.Blogs
{
    [AllowAnonymous]
    public class BlogModel : BaseModel
    {
        [BindProperty]
        public InputComment InputComment { get; set; }
        public Blog Blog { get; set; }
        public BlogModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager) : base (context, authorizationService, userManager)
        {

        }
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Blog = await Context.Blog
                .Include(blog => blog.Comments)
                .FirstOrDefaultAsync(blog => blog.ID == id);
            if (Blog == null)
                return NotFound();
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ERROR");
            }
            var user = await UserManager.GetUserAsync(User);
            var comment = new Comment
            {
                Author = user.UserName,
                Content = InputComment.Content,
                Date = DateTime.Now,
                BlogID = InputComment.BlogID
            };

            Context.Add(comment);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Blog", new { id = comment.BlogID });
        }
        public async Task<IActionResult> OnPostDeleteBlogAsync(int blogID)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Challenge();
            }

            var user = await UserManager.GetUserAsync(User);
            var blog = await Context.Blog.FindAsync(blogID);
            if (user.UserName != blog.Author)
            {
                return Forbid();
            }

            Context.Blog.Remove(blog);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

    }
    public class InputComment
    { 
        public string Content { get; set; }
        public int BlogID { get; set; }
    }

}
