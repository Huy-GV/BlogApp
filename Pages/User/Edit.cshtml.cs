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

namespace BlogApp.Pages.User
{
    public class EditUserModel
    {
        public string UserName { get; set; }
        [Required]
        public string Country { get; set; }
        public string Occupation { get; set; }
        public string Description { get; set; }
        public IFormFile ProfilePicture { get; set; }
    }


    [Authorize]
    public class EditModel : BaseModel
    {
        [BindProperty]
        public EditUserModel EditUser { get; set; }
        private readonly IWebHostEnvironment _webHostEnv;
        private readonly ILogger<EditModel> _logger;
        public EditModel(      
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<EditModel> logger,
            IWebHostEnvironment webHostEnv) : base(context, userManager)
        {
            _webHostEnv = webHostEnv;
            _logger = logger;
        }
        public async Task<IActionResult> OnGetAsync(string username)
        {
            var user = await UserManager.FindByNameAsync(username);
            if (user == null)
                return NotFound();
            if (user.UserName != User.Identity.Name)
                return Forbid();

            EditUser = new EditUserModel()
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

            var applicationUser = await Context.ApplicationUser.FindAsync(user.Id);
            applicationUser.Description = EditUser.Description;
            applicationUser.Country = EditUser.Country ?? "Australia";
            if (EditUser.ProfilePicture != null) 
            {
                RemoveOldProfilePicture(applicationUser.ProfilePicture);
                applicationUser.ProfilePicture = await GetProfilePicturePath(EditUser);
            }

            Context.Attach(applicationUser).State = EntityState.Modified;
            await Context.SaveChangesAsync();

            return RedirectToPage("/User/Index", new { username = EditUser.UserName });
        }
        //TODO: write a service for this?
        private async Task<string> GetProfilePicturePath(EditUserModel editUser) 
        {
            string fileName = "";
            //TODO: santinise file name by removing file paths
            //TODO: get config string from json
            try
            {
                string directoryPath = Path.Combine(_webHostEnv.WebRootPath, "images", "profiles");
                fileName = DateTime.Now.Ticks.ToString() + "_" + editUser.ProfilePicture.FileName;
                string filePath = Path.Combine(directoryPath, fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await editUser.ProfilePicture.CopyToAsync(stream);
                }
            } catch (Exception ex)
            {
                _logger.LogError($"Failed to upload new profile picture: {ex}");
            }


            return fileName;
        }
        private void RemoveOldProfilePicture(string oldFileName)
        {
            if (oldFileName != "default" && oldFileName != string.Empty) 
            {
                string directoryPath = Path.Combine(_webHostEnv.WebRootPath, "images", "profiles");
                string filePath = Path.Combine(directoryPath, oldFileName);
                try 
                {
                    System.IO.File.Delete(filePath);        
                } catch 
                {
                    _logger.LogError($"Failed to remove profile picture with file path: ${filePath}");
                }
                
            }
        }
    }
}
