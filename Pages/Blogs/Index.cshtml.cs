using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorBlog.Data;
using RazorBlog.Data.Constants;
using RazorBlog.Data.Dtos;
using RazorBlog.Models;

namespace RazorBlog.Pages.Blogs;

[AllowAnonymous]
public class IndexModel : PageModel
{
    private readonly RazorBlogDbContext _context;

    public IndexModel(RazorBlogDbContext context)
    {
        _context = context;
    }

    [BindProperty] public IEnumerable<BlogDto> Blogs { get; set; }

    [BindProperty(SupportsGet = true)] public string SearchString { get; set; }

    public async Task OnGetAsync()
    {
        SearchString = SearchString?.Trim().Trim(' ') ?? string.Empty;
        Blogs = await _context.Blog
            .Include(b => b.AppUser)
            .Include(b => b.Comments)
            .ThenInclude(c => c.AppUser)
            .Select(b => new BlogDto
            {
                Id = b.Id,
                Title = b.IsHidden ? RemovedContent.ReplacementText : b.Title,
                AuthorName = b.AppUser == null
                    ? "Deleted User"
                    : b.AppUser.UserName,
                CreatedDate = b.Date,
                ViewCount = b.ViewCount,
                Date = b.Date,
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