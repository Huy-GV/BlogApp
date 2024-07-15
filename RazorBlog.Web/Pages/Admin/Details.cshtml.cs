using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RazorBlog.Core.Data;
using RazorBlog.Core.Data.Constants;
using RazorBlog.Core.Models;
using RazorBlog.Core.WriteServices;

namespace RazorBlog.Web.Pages.Admin;

[Authorize(Roles = Roles.AdminRole)]
public class DetailsModel : RichPageModelBase<DetailsModel>
{
    private readonly IUserModerationService _userModerationService;

    public DetailsModel(RazorBlogDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<DetailsModel> logger,
        IUserModerationService userUserModerationService,
        IPostModerationService postModerationService,
        IPostDeletionScheduler postDeletionService) : base(context, userManager, logger)
    {
        _userModerationService = userUserModerationService;
    }

    [BindProperty(SupportsGet = true)]
    public BanTicket? CurrentBanTicket { get; set; }

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
        CurrentBanTicket = await _userModerationService.FindBanTicketByUserNameAsync(userName);

        return Page();
    }
}
