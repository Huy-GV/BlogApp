using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using BlogApp.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using BlogApp.Services;
using BlogApp.Data.ViewModel;

namespace BlogApp.Pages.Authentication
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly ImageFileService _imageFileService;
        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            ImageFileService imageFileService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _imageFileService = imageFileService;
        }

        [BindProperty]
        public CreateUserViewModel CreateUser { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            string profilePath = "default";


            if (ModelState.IsValid)
            {
                if (CreateUser.ProfilePicture != null) 
                {
                    profilePath = await GetProfilePicturePath(CreateUser);
                } 
                var user = new ApplicationUser
                {
                    UserName = CreateUser.UserName,
                    EmailConfirmed = true,
                    RegistrationDate = DateTime.Now,
                    ProfilePicture = profilePath,
                    Country = "Australia"
                };
                var result = await _userManager.CreateAsync(user, CreateUser.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
            }

            return Page();
        }
        private async Task<string> GetProfilePicturePath(CreateUserViewModel createUser) 
        {
            string fileName = "";
            try
            {
                fileName = await _imageFileService.UploadProfileImageAsync(createUser.ProfilePicture);
            } catch (Exception ex)
            {
                _logger.LogError($"Failed to upload new profile picture: {ex}");
            }
            return fileName;
        }
    }
}
