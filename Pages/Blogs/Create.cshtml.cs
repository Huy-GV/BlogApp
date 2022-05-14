using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RazorBlog.Data;
using RazorBlog.Data.ViewModels;
using RazorBlog.Models;
using RazorBlog.Services;

namespace RazorBlog.Pages.Blogs;

[Authorize]
public class CreateModel : BasePageModel<CreateModel>
{
    private readonly IImageStorage _imageStorage;

    private readonly IUserModerationService _userModerationService;

    public CreateModel(
        RazorBlogDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<CreateModel> logger,
        IImageStorage imageService,
        IUserModerationService userModerationService) : base(
    context, userManager, logger)
    {
        _imageStorage = imageService;
        _userModerationService = userModerationService;
    }

    [BindProperty] public BlogViewModel CreateBlogViewModel { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await GetUserAsync();
        var username = user.UserName;

        if (await _userModerationService.BanTicketExistsAsync(username)) return RedirectToPage("./Index");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await GetUserAsync();
        if (await _userModerationService.BanTicketExistsAsync(user.UserName)) return Forbid();

        if (!ModelState.IsValid)
        {
            Logger.LogError("Invalid model state when submitting new blog.");
            return Page();
        }

        try
        {
            var imageName = await _imageStorage.UploadBlogCoverImageAsync(CreateBlogViewModel.CoverImage);
            var newBlog = new Blog
            {
                CoverImageUri = imageName,
                Date = DateTime.Now,
                AppUserId = user.Id
            };

            DbContext.Blog.Add(newBlog).CurrentValues.SetValues(CreateBlogViewModel);
            await DbContext.SaveChangesAsync();

            // todo: redirect to blog page
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to create blog");
            Logger.LogError(ex.Message);

            return Page();
        }
    }
}