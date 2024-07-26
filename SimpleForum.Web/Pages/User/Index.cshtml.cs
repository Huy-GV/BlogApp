using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimpleForum.Core.Data;
using SimpleForum.Core.Models;
using SimpleForum.Core.ReadServices;

namespace SimpleForum.Web.Pages.User;

[Authorize]
public class IndexModel : RichPageModelBase<IndexModel>
{
    private readonly IAggregateImageUriResolver _aggregateImageUriResolver;
    private readonly IDefaultProfileImageProvider _defaultProfileImageProvider;

    public IndexModel(SimpleForumDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<IndexModel> logger,
        IAggregateImageUriResolver aggregateImageUriResolver,
        IDefaultProfileImageProvider defaultProfileImageProvider) : base(context, userManager, logger)
    {
        _aggregateImageUriResolver = aggregateImageUriResolver;
        _defaultProfileImageProvider = defaultProfileImageProvider;
    }

    public IActionResult OnGet()
    {
        return Page();
    }
}
