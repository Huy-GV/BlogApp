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
using BlogApp.Interfaces;

namespace BlogApp.Pages.User
{
    [Authorize]
    public class EditModel : BasePageModel<EditModel>
    {
        [BindProperty]
        public EditUserViewModel EditUserViewModel { get; set; }
        private readonly ILogger<EditModel> _logger;
        private readonly IImageService _imageService;
        public EditModel(      
            RazorBlogDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<EditModel> logger,
            IImageService imageService) : base(context, userManager, logger)
        {
            _logger = logger;
            _imageService = imageService;
        }
        public async Task<IActionResult> OnGetAsync(string? username)
        {
            if (username == null)
            {
                return NotFound();
            }

            var user = await UserManager.FindByNameAsync(username);
            if (user == null)
            {
                return NotFound();
            }


            if (user.UserName != User.Identity.Name)
            {
                return Forbid();
            }

            EditUserViewModel = new EditUserViewModel()
            {
                UserName = username,
                Country = user.Country,
                Description = user.Description
            };

            return Page();
        }
        public async Task<IActionResult> OnPostAsync() 
        {   
            var user = await UserManager.FindByNameAsync(EditUserViewModel.UserName);
            if (user == null)
            {
                return NotFound();
            }

            if (user.UserName != User.Identity.Name)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany( m => m.Errors)
                    .Select(e => e.ErrorMessage);

                foreach (var error in errors)
                {
                    _logger.LogError(error);
                }

                return RedirectToPage("/User/Edit", new { username = EditUserViewModel.UserName });
            }

            var applicationUser = await DbContext.ApplicationUser.FindAsync(user.Id);
            DbContext.Attach(applicationUser).CurrentValues.SetValues(EditUserViewModel);

            if (EditUserViewModel.NewProfilePicture != null) 
            {
                _imageService.DeleteImage(applicationUser.ProfilePicture);
                applicationUser.ProfilePicture = await 
                    GetProfilePicturePath(EditUserViewModel);
            }

            DbContext.Attach(applicationUser).State = EntityState.Modified;
            await DbContext.SaveChangesAsync();

            return RedirectToPage("/User/Index", new { username = EditUserViewModel.UserName });
        }
        private async Task<string> GetProfilePicturePath(EditUserViewModel editUser) 
        {
            string fileName = string.Empty;
            try
            {
                fileName = _imageService.BuildFileName(editUser.NewProfilePicture.FileName);
                await _imageService.UploadProfileImageAsync(EditUserViewModel.NewProfilePicture, fileName);
            } 
            catch (Exception ex)
            {
                _logger.LogError($"Failed to upload new profile picture: {ex}");
            }

            return fileName;
        }
    }
}
