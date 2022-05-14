using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RazorBlog.Data;
using RazorBlog.Data.DTOs;
using RazorBlog.Models;

namespace RazorBlog.Pages.User;

[Authorize]
public class IndexModel : BasePageModel<IndexModel>
{
    public IndexModel(
        RazorBlogDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<IndexModel> logger) : base(context, userManager, logger)
    {
    }

    [BindProperty] public PersonalProfileDto UserDto { get; set; }

    public async Task<IActionResult> OnGetAsync(string? username)
    {
        if (username == null) return NotFound();

        var user = await UserManager.FindByNameAsync(username);
        if (user == null) return NotFound();

        if (user.UserName != User.Identity?.Name) return Forbid();

        var blogs = DbContext.Blog
            .Include(b => b.AppUser)
            .AsNoTracking()
            .Where(blog => blog.AppUser.UserName == username)
            .ToList();

        UserDto = new PersonalProfileDto
        {
            UserName = username,
            BlogCount = (uint)blogs.Count,
            ProfilePicturePath = user.ProfileImageUri,
            Blogs = blogs,
            Description = user.Description,
            CommentCount = (uint)DbContext.Comment
                .Include(c => c.AppUser)
                .Where(c => c.AppUser.UserName == username)
                .ToList()
                .Count,
            BlogCountCurrentYear = (uint)blogs
                .Where(blog => blog.AppUser.UserName == username &&
                               blog.Date.Year == DateTime.Now.Year)
                .ToList()
                .Count,
            ViewCountCurrentYear = (uint)blogs
                .Where(blog => blog.AppUser.UserName == username &&
                               blog.Date.Year == DateTime.Now.Year)
                .Sum(blogs => blogs.ViewCount),
            RegistrationDate = user.RegistrationDate?.ToString("dd MM yyyy") ?? ""
        };

        return Page();
    }
}