using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BlogApp.Data;
using BlogApp.Models;
using Microsoft.AspNetCore.Identity;
using BlogApp.Pages;

namespace BlogApp.Pages.Blogs
{
    [AllowAnonymous]
    public class IndexModel : BaseModel
    {
        public IList<Blog> Blog { get; set; }
        public IndexModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager) : base(context, userManager)
        {

        }
        public async Task OnGetAsync()
        {
            Blog = await Context
                .Blog
                .Include(blog => blog.Comments)
                .ToListAsync();
        }
    }
}
