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
namespace BlogApp.Pages.Blogs
{
    [Authorize]
    public class CreateModel : BaseModel<CreateModel>
    {
        [BindProperty]
        public CreateBlogViewModel CreateBlogVM { get; set; }
        private readonly UserSuspensionService _suspensionService;
        private readonly ImageFileService _imageFileService;
        public CreateModel(
            RazorBlogDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<CreateModel> logger,
            ImageFileService imageFileService,
            UserSuspensionService suspensionService) : base(
                context, userManager, logger)
        {
            _imageFileService = imageFileService;
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

            var entry = DbContext.Blog.Add(new Blog()
            {
                ImagePath = await _imageFileService
                .UploadBlogImageAsync(CreateBlogVM.CoverImage),
                Date = DateTime.Now,
                Author = user.UserName,
                AppUserID = user.Id
            });

            entry.CurrentValues.SetValues(CreateBlogVM);
            await DbContext.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }

}

