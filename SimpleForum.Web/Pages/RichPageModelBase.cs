using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SimpleForum.Core.Data;
using SimpleForum.Core.Models;

namespace SimpleForum.Web.Pages;

public class RichPageModelBase<TPageModel> : PageModel where TPageModel : PageModel
{
    public RichPageModelBase(SimpleForumDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<TPageModel> logger)
    {
        DbContext = context;
        UserManager = userManager;
        Logger = logger;
    }

    protected SimpleForumDbContext DbContext { get; }

    protected UserManager<ApplicationUser> UserManager { get; }

    protected ILogger<TPageModel> Logger { get; }

    protected async Task<ApplicationUser?> GetUserOrDefaultAsync()
    {
        return await UserManager.GetUserAsync(User);
    }

    protected bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;
}
