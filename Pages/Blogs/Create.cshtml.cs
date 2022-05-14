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

        private readonly IUserModerationService _userModerationService;
        private readonly IImageStorage _imageStorage;

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

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await GetUserAsync();
            var username = user.UserName;

            if (await _userModerationService.BanTicketExistsAsync(username))
            {
                return RedirectToPage("./Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await GetUserAsync();
            if (await _userModerationService.BanTicketExistsAsync(user.UserName))
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
                var newBlog = new Blog()
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
}