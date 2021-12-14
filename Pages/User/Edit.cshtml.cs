using BlogApp.Data;
using BlogApp.Data.DTOs;
using BlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Logging;
using BlogApp.Services;
using BlogApp.Data.ViewModel;

namespace BlogApp.Pages.User
{
    [Authorize]
    public class EditModel : BaseModel
    {
        [BindProperty]
        public EditUserViewModel EditUserVM { get; set; }
        private readonly ILogger<EditModel> _logger;
        private readonly ImageFileService _imageFileService;
        public EditModel(      
            RazorBlogDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<EditModel> logger,
            ImageFileService imageFileService) : base(context, userManager)
        {
            _logger = logger;
            _imageFileService = imageFileService;
        }
        public async Task<IActionResult> OnGetAsync(string username)
        {
            var user = await UserManager.FindByNameAsync(username);
            if (user == null)
                return NotFound();
            if (user.UserName != User.Identity.Name)
                return Forbid();

            EditUserVM = new EditUserViewModel()
            {
                UserName = username,
                Country = user.Country,
                Description = user.Description
            };

            return Page();
        }
        public async Task<IActionResult> OnPostAsync() 
        {   
            _logger.LogInformation($"User named {EditUserVM.UserName} attempted to update their profile");
            var user = await UserManager.FindByNameAsync(EditUserVM.UserName);
            if (user == null)
                return NotFound();
            if (user.UserName != User.Identity.Name)
                return Forbid();
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state when editing blog");
                return Page();
            }

            var applicationUser = await DbContext.ApplicationUser.FindAsync(user.Id);
            DbContext.Attach(applicationUser).CurrentValues.SetValues(EditUserVM);

            if (EditUserVM.NewProfilePicture != null) 
            {
                _imageFileService.DeleteImage(applicationUser.ProfilePicture);
                applicationUser.ProfilePicture = await 
                    GetProfilePicturePath(EditUserVM);
            }

            DbContext.Attach(applicationUser).State = EntityState.Modified;
            await DbContext.SaveChangesAsync();

            return RedirectToPage("/User/Index", new { username = EditUserVM.UserName });
        }
        private async Task<string> GetProfilePicturePath(EditUserViewModel editUser) 
        {
            string fileName = "";
            try
            {
                fileName = await _imageFileService.UploadProfileImageAsync(EditUserVM.NewProfilePicture);
            } catch (Exception ex)
            {
                _logger.LogError($"Failed to upload new profile picture: {ex}");
            }
            return fileName;
        }
    }
}
