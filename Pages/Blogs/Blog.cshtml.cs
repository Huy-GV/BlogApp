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
        [BindProperty]
        public EditForm EditForm { get; set; }
        [BindProperty]
        public EditComment EditComment { get; set; }

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
        public async Task<IActionResult> OnPostEditBlogAsync(int blogID)
        {
            var user = await UserManager.GetUserAsync(User);
            var blog = await Context.Blog.FindAsync(blogID);

            if (EditForm.Content == "") return RedirectToPage("./Blog", new { id = blogID });

            blog.Content = EditForm.Content;

            if (!User.Identity.IsAuthenticated)
            {
                return Challenge();
            }

            if (user.UserName != blog.Author)
            {
                return Forbid();
            }

            Context.Attach(blog).State = EntityState.Modified;
            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                    throw;
            }

            return RedirectToPage("./Blog", new { id = blogID });


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

        public async Task<IActionResult> OnPostDeleteCommentAsync(int commentID)
        {
            //TODO if this doesnt work, write a linq query on Context.Blog
            var user = await UserManager.GetUserAsync(User);
            var comment = await Context.Comment.FindAsync(commentID);
            
            if (user.UserName != comment.Author)
            {
                return Forbid();
            }
            Context.Comment.Remove(comment);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Blog", new { id = comment.BlogID });
        }

        public async Task<IActionResult> OnPostEditCommentAsync(int commentID)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ERROR");
            }
            Context.Attach(EditComment).State = EntityState.Modified;
            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            var comment = await Context.Comment.FindAsync(commentID);
            return RedirectToPage("./Blog", new { id = comment.BlogID });
        }

    }
    public class InputComment
    { 
        public string Content { get; set; }
        public int BlogID { get; set; }
    }
    public class EditForm
    { 
        public string Content { get; set; }
    }

    public class EditComment 
    {
        public string Content {get; set;}
    }

}
