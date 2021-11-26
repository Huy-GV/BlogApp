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
using BlogApp.Data.FormModels;

namespace BlogApp.Pages.User
{
    [Authorize]
    public class EditModel : BaseModel
    {
        [BindProperty]
        public EditUser EditUser { get; set; }
        private readonly ILogger<EditModel> _logger;
        private readonly ImageFileService _imageFileService;
        public EditModel(      
            ApplicationDbContext context,
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

            EditUser = new EditUser()
            {
                UserName = username,
                Country = user.Country,
                Description = user.Description
            };

            return Page();
        }
        public async Task<IActionResult> OnPostAsync() 
        {   
            _logger.LogInformation($"User named {EditUser.UserName} attempted to update their profile");
            var user = await UserManager.FindByNameAsync(EditUser.UserName);
            if (user == null)
                return NotFound();
            if (user.UserName != User.Identity.Name)
                return Forbid();
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state when editing blog");
                return Page();
            }

            var applicationUser = await Context.ApplicationUser.FindAsync(user.Id);
            applicationUser.Description = EditUser.Description;
            applicationUser.Country = EditUser.Country ?? "Australia";
            if (EditUser.ProfilePicture != null) 
            {
                _imageFileService.DeleteImage(applicationUser.ProfilePicture);
                applicationUser.ProfilePicture = await GetProfilePicturePath(EditUser);
            }

            Context.Attach(applicationUser).State = EntityState.Modified;
            await Context.SaveChangesAsync();

            return RedirectToPage("/User/Index", new { username = EditUser.UserName });
        }
        private async Task<string> GetProfilePicturePath(EditUser editUser) 
        {
            string fileName = "";
            try
            {
                fileName = await _imageFileService.UploadProfileImageAsync(EditUser.ProfilePicture);
            } catch (Exception ex)
            {
                _logger.LogError($"Failed to upload new profile picture: {ex}");
            }
            return fileName;
        }
    }
}
