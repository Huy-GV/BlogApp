using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogApp.Data;
using BlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BlogApp.Services;
using BlogApp.Data.DTOs;

namespace BlogApp.Pages.Blogs
{
    [Authorize]
    public class CreateModel : BasePageModel<CreateModel>
    {
        [BindProperty]
        public BlogViewModel CreateBlogViewModel { get; set; }

        private readonly UserModerationService _suspensionService;
        private readonly IImageStorage _imageStorage;

        public CreateModel(
            RazorBlogDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<CreateModel> logger,
            IImageStorage imageService,
            UserModerationService suspensionService) : base(
                context, userManager, logger)
        {
            _imageStorage = imageService;
            _suspensionService = suspensionService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await UserManager.GetUserAsync(User);
            var username = user.UserName;

            if (await _suspensionService.BanTicketExistsAsync(username))
            {
                return RedirectToPage("./Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await UserManager.GetUserAsync(User);
            if (await _suspensionService.BanTicketExistsAsync(user.UserName))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                Logger.LogError("Invalid model state when submitting new blog.");
                return Page();
            }

            try
            {
                var imageName = await _imageStorage.UploadBlogCoverImageAsync(CreateBlogViewModel.CoverImage);
                var entry = DbContext.Blog.Add(new Blog()
                {
                    CoverImageUri = imageName,
                    Date = DateTime.Now,
                    AppUserId = user.Id
                });

                entry.CurrentValues.SetValues(CreateBlogViewModel);
                await DbContext.SaveChangesAsync();

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
}