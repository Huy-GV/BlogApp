using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlogApp.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlogApp.Models;
using Microsoft.Extensions.Logging;

namespace BlogApp.Pages
{
    public class BasePageModel<TPageModel> : PageModel where TPageModel : PageModel
    {
        protected RazorBlogDbContext DbContext { get; }
        protected UserManager<ApplicationUser> UserManager { get; }
        protected ILogger<TPageModel> Logger { get;  }
        public BasePageModel(
            RazorBlogDbContext context, 
            UserManager<ApplicationUser> userManager,
            ILogger<TPageModel> logger)
        {
            DbContext = context;
            UserManager = userManager;
            Logger = logger;
        }

        protected async Task<ApplicationUser> GetUserAsync() => await UserManager.GetUserAsync(User);
    }
}
