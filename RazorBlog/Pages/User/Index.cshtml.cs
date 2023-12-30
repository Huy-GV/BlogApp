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
public class IndexModel(
    RazorBlogDbContext context,
    UserManager<ApplicationUser> userManager,
    ILogger<IndexModel> logger) : RichPageModelBase<IndexModel>(context, userManager, logger)
{
    [BindProperty] public PersonalProfileDto UserDto { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(string? userName)
    {
        if (userName == null)
        {
            return NotFound();
        }

        var user = await GetUserOrDefaultAsync();
        if (user == null || user.UserName == null || user.UserName != User.Identity?.Name)
        {
            return Forbid();
        }

        var blogs = DbContext.Blog
            .Include(b => b.AppUser)
            .AsNoTracking()
            .Where(blog => blog.AppUser.UserName == userName)
            .ToList();

        var groups = blogs.GroupBy(b => b.CreationTime.Year).OrderByDescending(g => g.Key);
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
                    CreationTime = b.CreationTime,
                }).ToList());
        }

        UserDto = new PersonalProfileDto
        {
            UserName = userName,
            BlogCount = (uint)blogs.Count,
            ProfileImageUri = user.ProfileImageUri,
            BlogsGroupedByYear = blogsGroupedByYear,
            Description = string.IsNullOrEmpty(user.Description)
                ? "None"
                : user.Description,
            CommentCount = (uint)DbContext.Comment
                .Include(c => c.AppUser)
                .Where(c => c.AppUser.UserName == userName)
                .ToList()
                .Count,
            BlogCountCurrentYear = (uint)blogs
                .Where(blog => blog.AppUser.UserName == userName &&
                               blog.CreationTime.Year == DateTime.Now.Year)
                .ToList()
                .Count,
            ViewCountCurrentYear = (uint)blogs
                .Where(blog => blog.AppUser.UserName == userName &&
                               blog.CreationTime.Year == DateTime.Now.Year)
                .Sum(blogs => blogs.ViewCount),
            RegistrationDate = user.RegistrationDate == null
                    ? "a long time ago"
                    : user.RegistrationDate.Value.ToString("dd/MMMM/yyyy"),
        };

        return Page();
    }
}