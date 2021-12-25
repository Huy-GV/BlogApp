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
using BlogApp.Data.ViewModel;
using BlogApp.Interfaces;

namespace BlogApp.Pages.Blogs
{
    [Authorize]
    public class CreateModel : BasePageModel<CreateModel>
    {
        [BindProperty]
        public CreateBlogViewModel CreateBlogVM { get; set; }
        private readonly UserModerationService _suspensionService;
        private readonly IImageService _imageService;
        public CreateModel(
            RazorBlogDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<CreateModel> logger,
            IImageService imageService,
            UserModerationService suspensionService) : base(
                context, userManager, logger)
        {
            _imageService = imageService;
            _suspensionService = suspensionService;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await UserManager.GetUserAsync(User);
            var username = user.UserName;

            if (await _suspensionService.ExistsAsync(username))
                return RedirectToPage("./Index");

            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await UserManager.GetUserAsync(User);
            var username = user.UserName;

            if (await _suspensionService.ExistsAsync(username))
                return RedirectToPage("./Index");

            if (!ModelState.IsValid)
            {
                Logger.LogError("Invalid model state when submitting new post");
                return Page();
            }
            try
            {
                var coverImage = CreateBlogVM.CoverImage;
                var imageName = _imageService.BuildFileName(coverImage.FileName);
                await _imageService.UploadBlogImageAsync(coverImage, imageName);
                DbContext.Blog.Add(new Blog()
                    {
                        ImagePath = imageName,
                        Date = DateTime.Now,
                        Author = user.UserName,
                        AppUserID = user.Id 
                    })
                    .CurrentValues.SetValues(CreateBlogVM);
                await DbContext.SaveChangesAsync();
                return RedirectToPage("./Index");
            } catch (Exception ex)
            {
                Logger.LogError("Failed to create blog");
                Logger.LogError(ex.Message);
                return Page();
            }

            
        }
    }

}

