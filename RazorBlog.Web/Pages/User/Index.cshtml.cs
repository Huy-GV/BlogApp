using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RazorBlog.Core.Data;
using RazorBlog.Core.Data.Dtos;
using RazorBlog.Core.Models;
using RazorBlog.Core.ReadServices;

namespace RazorBlog.Web.Pages.User;

[Authorize]
public class IndexModel : RichPageModelBase<IndexModel>
{
    private readonly IAggregateImageUriResolver _aggregateImageUriResolver;
    private readonly IDefaultProfileImageProvider _defaultProfileImageProvider;

    public IndexModel(RazorBlogDbContext context,
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
