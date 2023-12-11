using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RazorBlog.Data;
using RazorBlog.Data.Constants;
using RazorBlog.Data.Dtos;
using RazorBlog.Models;

namespace RazorBlog.Pages.Blogs;

[AllowAnonymous]
public class IndexModel : BasePageModel<IndexModel>
{
    public IndexModel(
        RazorBlogDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<IndexModel> logger) : base(context, userManager, logger)
    {
    }

    [BindProperty] public IEnumerable<BlogDto> Blogs { get; set; }

    [BindProperty(SupportsGet = true)] public string SearchString { get; set; }

    public async Task OnGetAsync()
    {
        SearchString = SearchString?.Trim().Trim(' ') ?? string.Empty;
        Blogs = await DbContext.Blog
            .Include(b => b.AppUser)
            .Include(b => b.Comments)
            .ThenInclude(c => c.AppUser)
            .Select(b => new BlogDto
            {
                Id = b.Id,
                Title = b.IsHidden ? RemovedContent.ReplacementText : b.Title,
                AuthorName = b.AppUser == null
                    ? RemovedContent.ReplacementUserName
                    : b.AppUser.UserName,
                CreationTime = b.CreationTime,
                LastUpdateTime = b.LastUpdateTime,
                ViewCount = b.ViewCount,
                CoverImageUri = b.CoverImageUri,
                Introduction = b.IsHidden ? RemovedContent.ReplacementText : b.Introduction
            })
            .Where(b => SearchString == null ||
                        SearchString == string.Empty ||
                        b.Title.Contains(SearchString) ||
                        b.AuthorName.Contains(SearchString))
            .Take(10)
            .ToListAsync();
    }
}