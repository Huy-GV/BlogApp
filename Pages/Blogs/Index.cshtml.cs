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
public class IndexModel(
    RazorBlogDbContext context,
    UserManager<ApplicationUser> userManager,
    ILogger<IndexModel> logger) : BasePageModel<IndexModel>(context, userManager, logger)
{
    [BindProperty] 
    public IEnumerable<BlogDto> Blogs { get; set; } = Enumerable.Empty<BlogDto>();

    [BindProperty(SupportsGet = true)]
    public string SearchString { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        SearchString = SearchString?.Trim().Trim(' ') ?? string.Empty;
        Blogs = await DbContext.Blog
            .Include(b => b.AppUser)
            .Include(b => b.Comments)
            .ThenInclude(c => c.AppUser)
            .Where(x => !x.IsHidden)
            .Select(b => new BlogDto
            {
                Id = b.Id,
                Title = b.IsHidden ? ReplacementText.HiddenContent : b.Title,
                AuthorName = b.AppUser == null
                    ? ReplacementText.DeletedUser
                    : b.AppUser.UserName!,
                CreationTime = b.CreationTime,
                LastUpdateTime = b.LastUpdateTime,
                ViewCount = b.ViewCount,
                CoverImageUri = b.CoverImageUri,
                Introduction = b.IsHidden ? ReplacementText.HiddenContent : b.Introduction
            })
            .Where(b => SearchString == null ||
                        SearchString == string.Empty ||
                        b.Title.Contains(SearchString) ||
                        b.AuthorName.Contains(SearchString))
            .Take(10)
            .ToListAsync();
    }
}