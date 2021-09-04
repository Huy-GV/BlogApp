using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlogApp.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlogApp.Pages
{
    public class BaseModel : PageModel
    {
        protected ApplicationDbContext Context { get; }
        protected IAuthorizationService AuthorizationService { get; }
        protected UserManager<IdentityUser> UserManager { get; }
        public BaseModel(
            ApplicationDbContext context, 
            UserManager<IdentityUser> userManager)
        {
            Context = context;
            UserManager = userManager;
        }

        protected async Task<bool> SuspensionExists(string username)
        {
            await CheckSuspensionExpiry(username);
            return Context.Suspension.Any(s => s.Username == username);
        }
        protected async Task CheckSuspensionExpiry(string username)
        {
            var suspension = Context.Suspension.FirstOrDefault(s => s.Username == username);
            if (suspension != null && DateTime.Compare(DateTime.Now, suspension.Expiry) > 0)
            {
                Context.Remove(suspension);
                await Context.SaveChangesAsync();
            }
        }

    }
}
