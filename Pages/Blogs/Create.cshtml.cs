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
    public class CreateModel : BaseModel
    {
        [BindProperty]
        public CreateBlogViewModel CreateBlogVM { get; set; }
        private readonly ILogger<CreateModel> _logger;
        private readonly UserSuspensionService _suspensionService;
        private readonly ImageFileService _imageFileService;
        public CreateModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<CreateModel> logger,
            ImageFileService imageFileService,
            UserSuspensionService suspensionService) : base(context, userManager)
        {
            _logger = logger;
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
                _logger.LogError("Invalid model state when submitting new post");
                return Page();
            }

            var entry = Context.Blog.Add(new Blog()
            {
                ImagePath = await _imageFileService
                .UploadBlogImageAsync(CreateBlogVM.CoverImage),
                Date = DateTime.Now,
                Author = user.UserName
            });

            entry.CurrentValues.SetValues(CreateBlogVM);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }

}

