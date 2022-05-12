using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BlogApp.Data;
using BlogApp.Models;
using Microsoft.AspNetCore.Identity;
using BlogApp.Pages;
using Microsoft.Extensions.Logging;
using RazorBlog.Data.DTOs;

namespace BlogApp.Pages.Blogs
{
    [AllowAnonymous]
    public class IndexModel : BasePageModel<IndexModel>
    {
        public IEnumerable<BlogDto> Blogs { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        public IndexModel(
            RazorBlogDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<IndexModel> logger) : base(
                context, userManager, logger)
        {
        }

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
                    Title = b.Title,
                    AuthorName = b.AppUser == null
                        ? "Deleted User"
                        : b.AppUser.UserName,
                    CreatedDate = b.Date,
                    ViewCount = b.ViewCount,
                    Date = b.Date,
                    CoverImageUri = b.CoverImageUri,
                    Introduction = b.Introduction,
                })
                .Where(b => SearchString == null ||
                        SearchString == string.Empty ||
                        b.Title.Contains(SearchString) ||
                        b.AuthorName.Contains(SearchString))
                .Take(10)
                .ToListAsync();
        }
    }
}