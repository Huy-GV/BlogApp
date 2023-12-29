using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using RazorBlog.Data;
using RazorBlog.Models;

namespace RazorBlog.Pages;

public class BasePageModel<TPageModel>(
    RazorBlogDbContext context,
    UserManager<ApplicationUser> userManager,
    ILogger<TPageModel> logger) : PageModel where TPageModel : PageModel
{
    protected RazorBlogDbContext DbContext { get; } = context;
    protected UserManager<ApplicationUser> UserManager { get; } = userManager;
    protected ILogger<TPageModel> Logger { get; } = logger;

    protected async Task<ApplicationUser?> GetUserOrDefaultAsync()
    {
        return await UserManager.GetUserAsync(User);
    }
}