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
    public class BaseModel<ModelType> : PageModel where ModelType : PageModel
    {
        protected RazorBlogDbContext DbContext { get; }
        protected UserManager<ApplicationUser> UserManager { get; }
        protected ILogger<ModelType> Logger { get;  }
        public BaseModel(
            RazorBlogDbContext context, 
            UserManager<ApplicationUser> userManager,
            ILogger<ModelType> logger)
        {
            DbContext = context;
            UserManager = userManager;
            Logger = logger;
        }
    }
}
