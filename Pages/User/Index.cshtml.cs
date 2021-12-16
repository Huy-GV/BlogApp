using BlogApp.Data;
using BlogApp.Data.DTOs;
using BlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogApp.Pages.User
{
    [Authorize]
    public class IndexModel : BaseModel<IndexModel>
    {
        [BindProperty]
        public UserDTO UserDTO { get; set; }
        public IndexModel(      
            RazorBlogDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<IndexModel> logger) : base(
                context, userManager, logger)
        {

        }
        public async Task<IActionResult> OnGetAsync(string? username)
        {
            if (username == null)
                return NotFound();
            var user = await UserManager.FindByNameAsync(username);
            if (user == null)
                return NotFound();
            if (user.UserName != User.Identity.Name)
                return Forbid();

            var blogs = DbContext.Blog
                .AsNoTracking()
                .Where(blog => blog.Author == username)
                .ToList();

            UserDTO = new UserDTO()
            {
                Username = username,
                BlogCount = blogs.Count,
                ProfilePath = user.ProfilePicture,
                Blogs = blogs,
                Description = user.Description,
                CommentCount = DbContext.Comment
                    .Where(comment => comment.Author == username)
                    .ToList()
                    .Count,
                BlogCountCurrentYear = blogs
                    .Where(blog => blog.Author == username 
                    && blog.Date.Year == DateTime.Now.Year)
                    .ToList()
                    .Count,
                ViewCountCurrentYear = blogs
                    .Where(blog => blog.Author == username
                    && blog.Date.Year == DateTime.Now.Year)
                    .Sum(blogs => blogs.ViewCount),
                Country = user.Country,
                RegistrationDate = user.RegistrationDate?.ToString("dd MM yyyy") ?? "",
            };


            return Page();
        }
    }
}
