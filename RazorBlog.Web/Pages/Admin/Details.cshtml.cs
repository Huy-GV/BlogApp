using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RazorBlog.Core.Data;
using RazorBlog.Core.Data.Constants;
using RazorBlog.Core.Models;
using RazorBlog.Core.ReadServices;

namespace RazorBlog.Web.Pages.Admin;

[Authorize(Roles = Roles.AdminRole)]
public class DetailsModel : RichPageModelBase<DetailsModel>
{
    private readonly IBanTicketReader _banTicketReader;

    public DetailsModel(RazorBlogDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<DetailsModel> logger,
        IBanTicketReader banTicketReader) : base(context, userManager, logger)
    {
        _banTicketReader = banTicketReader;
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
        CurrentBanTicket = await _banTicketReader.FindBanTicketByUserNameAsync(userName);

        return Page();
    }
}
