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
namespace BlogApp.Pages
{
    public class BaseModel : PageModel
    {
        protected ApplicationDbContext Context { get; }
        // protected IAuthorizationService AuthorizationService { get; }
        protected UserManager<ApplicationUser> UserManager { get; }
        public BaseModel(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager)
        {
            Context = context;
            UserManager = userManager;
        }
    }
}
