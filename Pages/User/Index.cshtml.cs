using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RazorBlog.Data;
using RazorBlog.Data.Dtos;
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
        if (username == null)
        {
            return NotFound();
        }

        var user = await UserManager.FindByNameAsync(username);
        if (user == null)
        {
            return NotFound();
        }

        if (user.UserName != User.Identity?.Name)
        {
            return Forbid();
        }

        var blogs = DbContext.Blog
            .Include(b => b.AppUser)
            .AsNoTracking()
            .Where(blog => blog.AppUser.UserName == username)
            .ToList();

        var groups = blogs.GroupBy(b => b.Date.Year).OrderByDescending(g => g.Key);
        var blogsGroupedByYear = new Dictionary<uint, List<MinimalBlogDto>>();
        foreach (var group in groups)
        {
            blogsGroupedByYear.Add(
                (uint)group.Key,
                group.Select(b => new MinimalBlogDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    ViewCount = b.ViewCount,
                    Date = b.Date,
                }).ToList());
        }

        UserDto = new PersonalProfileDto
        {
            UserName = username,
            BlogCount = (uint)blogs.Count,
            ProfileImageUri = user.ProfileImageUri,
            BlogsGroupedByYear = blogsGroupedByYear,
            Description = string.IsNullOrEmpty(user.Description)
                ? "None"
                : user.Description,
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
            RegistrationDate = user.RegistrationDate == null
                    ? "a long time ago"
                    : user.RegistrationDate.Value.ToString("dd/MMMM/yyyy"),
        };

        return Page();
    }
}