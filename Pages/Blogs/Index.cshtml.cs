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

namespace BlogApp.Pages.Blogs
{
    [AllowAnonymous]
    public class IndexModel : BasePageModel<IndexModel>
    {
        public IEnumerable<Blog> Blogs { get; set; }
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
            IQueryable<Blog> blogs = from blog in DbContext.Blog
                    select blog;

            if (!string.IsNullOrEmpty(SearchString)) 
            {
                SearchString = SearchString.ToLower().Trim();
                blogs = from blog in blogs.AsNoTracking()
                        where blog.Title.ToLower().Contains(SearchString) 
                        || blog.Author.ToLower().Contains(SearchString)
                        select blog;
            }
            Blogs = await blogs.ToListAsync();
        }
    }
}
