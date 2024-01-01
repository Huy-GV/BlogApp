using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RazorBlog.Data;
using RazorBlog.Data.ViewModels;
using RazorBlog.Extensions;
using RazorBlog.Models;
using RazorBlog.Services;

namespace RazorBlog.Pages.Blogs;

[Authorize]
public class CreateModel(
    RazorBlogDbContext context,
    UserManager<ApplicationUser> userManager,
    IBlogContentManager blogContentManager,
    ILogger<CreateModel> logger,
    IPostModerationService userModerationService) : RichPageModelBase<CreateModel>(context, userManager, logger)
{
    private readonly IPostModerationService _postModerationService = userModerationService;
    private readonly IBlogContentManager _blogContentManager = blogContentManager;

    [BindProperty]
    public CreateBlogViewModel CreateBlogViewModel { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await GetUserOrDefaultAsync();
        if (user?.UserName == null)
        {
            return Page();
        }

        if (!await _postModerationService.IsUserAllowedToCreatePost(user.UserName))
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
            return Page();
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