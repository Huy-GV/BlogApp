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
        protected IAuthorizationService AuthorizationService { get; }
        protected UserManager<ApplicationUser> UserManager { get; }
        public BaseModel(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager)
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
            var suspension = await GetSuspension(username);
            if (suspension != null && DateTime.Compare(DateTime.Now, suspension.Expiry) > 0)
            {
                Context.Remove(suspension);
                await Context.SaveChangesAsync();
            }
        }
        protected async Task<Suspension> GetSuspension(string username) {
            return await Context.Suspension.FirstOrDefaultAsync(s => s.Username == username);
        }
    }
}
