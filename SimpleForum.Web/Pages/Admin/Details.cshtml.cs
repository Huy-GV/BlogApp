using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimpleForum.Core.Data;
using SimpleForum.Core.Data.Constants;
using SimpleForum.Core.Models;

namespace SimpleForum.Web.Pages.Admin;

[Authorize(Roles = Roles.AdminRole)]
public class DetailsModel : RichPageModelBase<DetailsModel>
{
    public DetailsModel(
        SimpleForumDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<DetailsModel> logger) : base(context, userManager, logger)
    {
    }

    [BindProperty(SupportsGet = true)]
    [Required]
    public string UserName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string? userName)
    {
        if (userName == null)
        {
            return NotFound();
        }

        var user = await UserManager.FindByNameAsync(userName);
        if (user == null)
        {
            Logger.LogInformation("User not found");
            return NotFound();
        }

        UserName = userName;

        return Page();
    }
}
