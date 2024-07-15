using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RazorBlog.Core.Data;
using RazorBlog.Core.Data.ViewModels;
using RazorBlog.Core.Models;
using RazorBlog.Core.ReadServices;
using RazorBlog.Core.WriteServices;
using RazorBlog.Web.Extensions;

namespace RazorBlog.Web.Pages.Blogs;

[Authorize]
public class CreateModel : RichPageModelBase<CreateModel>
{
    private readonly IUserPermissionValidator _userPermissionValidator;
    private readonly IBlogContentManager _blogContentManager;

    public CreateModel(RazorBlogDbContext context,
        UserManager<ApplicationUser> userManager,
        IBlogContentManager blogContentManager,
        ILogger<CreateModel> logger,
        IUserPermissionValidator userPermissionValidator) : base(context, userManager, logger)
    {
        _userPermissionValidator = userPermissionValidator;
        _blogContentManager = blogContentManager;
    }

    [BindProperty]
    public CreateBlogViewModel CreateBlogViewModel { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await GetUserOrDefaultAsync();
        if (user?.UserName == null)
        {
            return Forbid();
        }

        if (!await _userPermissionValidator.IsUserAllowedToCreatePostAsync(user.UserName))
        {
            return Forbid();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await GetUserOrDefaultAsync();
        if (user?.UserName == null)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            Logger.LogError("Invalid model state when submitting new blog.");
            return Page();
        }

        var (result, newBlogId) = await _blogContentManager.CreateBlogAsync(CreateBlogViewModel, user.UserName);

        return this.NavigateOnResult(
            result,
            () => RedirectToPage("/Blogs/Read", new { id = newBlogId }));
    }
}
