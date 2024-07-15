using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RazorBlog.Core.Data;
using RazorBlog.Core.Data.Dtos;
using RazorBlog.Core.Models;
using RazorBlog.Core.ReadServices;

namespace RazorBlog.Web.Pages.User;

[Authorize]
public class IndexModel : RichPageModelBase<IndexModel>
{
    private readonly IAggregateImageUriResolver _aggregateImageUriResolver;
    private readonly IDefaultProfileImageProvider _defaultProfileImageProvider;

    public IndexModel(RazorBlogDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<IndexModel> logger,
        IAggregateImageUriResolver aggregateImageUriResolver,
        IDefaultProfileImageProvider defaultProfileImageProvider) : base(context, userManager, logger)
    {
        _aggregateImageUriResolver = aggregateImageUriResolver;
        _defaultProfileImageProvider = defaultProfileImageProvider;
    }

    [BindProperty] public PersonalProfileDto UserDto { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(string? userName)
    {
        if (userName == null)
        {
            return NotFound();
        }

        var user = await GetUserOrDefaultAsync();
        if (user?.UserName == null || user.UserName != User.Identity?.Name)
        {
            return Forbid();
        }

        var blogs = DbContext.Blog
            .Include(b => b.AuthorUser)
            .AsNoTracking()
            .Where(blog => blog.AuthorUser.UserName == userName)
            .ToList();

        var blogsGroupedByYear = blogs
            .GroupBy(b => b.CreationTime.Year)
            .OrderByDescending(g => g.Key)
            .ToDictionary(
                group => (uint)group.Key,
                group => group.Select(b => new MinimalBlogDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    ViewCount = b.ViewCount,
                    CreationTime = b.CreationTime,
                })
            .ToList());

        UserDto = new PersonalProfileDto
        {
            UserName = userName,
            BlogCount = (uint)blogs.Count,
            ProfileImageUri = await _aggregateImageUriResolver.ResolveImageUriAsync(user.ProfileImageUri)
                              ?? await _defaultProfileImageProvider.GetDefaultProfileImageUriAsync(),
            BlogsGroupedByYear = blogsGroupedByYear,
            Description = string.IsNullOrEmpty(user.Description)
                ? "None"
                : user.Description,
            CommentCount = (uint)DbContext.Comment
                .Include(c => c.AuthorUser)
                .Where(c => c.AuthorUser.UserName == userName)
                .ToList()
                .Count,
            BlogCountCurrentYear = (uint)blogs
                .Where(blog => blog.AuthorUser.UserName == userName &&
                               blog.CreationTime.Year == DateTime.Now.Year)
                .ToList()
                .Count,
            ViewCountCurrentYear = (uint)blogs
                .Where(blog => blog.AuthorUser.UserName == userName &&
                               blog.CreationTime.Year == DateTime.Now.Year)
                .Sum(blog => blog.ViewCount),
            RegistrationDate = user.RegistrationDate == null
                    ? "a long time ago"
                    : user.RegistrationDate.Value.ToString("dd/MMMM/yyyy"),
        };

        return Page();
    }
}
