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
        private readonly IImageStorage _imageStorage;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IImageStorage imageStorage)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _imageStorage = imageStorage;
        }

        [BindProperty]
        public CreateUserViewModel CreateUserViewModel { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (!ModelState.IsValid)
            {
                return Page();
            }
            
            var user = new ApplicationUser()
            {
                UserName = CreateUserViewModel.UserName,
                EmailConfirmed = true,
                RegistrationDate = DateTime.Now,
                ProfileImageUri = CreateUserViewModel.ProfilePicture == null
                    ? GetDefaultProfileImageUri()
                    : await UploadProfileImage(CreateUserViewModel.ProfilePicture),
            };

            var result = await _userManager.CreateAsync(user, CreateUserViewModel.Password);
            if (!result.Succeeded)
            {
                return Page();
            }
            
            _logger.LogInformation($"User created a new account with username {CreateUserViewModel.UserName}.");
            await _signInManager.SignInAsync(user, isPersistent: false);

            return LocalRedirect(returnUrl);
        }

        private async Task<string> UploadProfileImage(IFormFile image)
        {
            try
            {
                return await _imageStorage.UploadProfileImageAsync(image);
            }
            catch (Exception ex)
            {
                // todo: un-hardcode the default image path
                _logger.LogError($"Failed to upload new profile picture: {ex}");
                return GetDefaultProfileImageUri();
            }
        }
        
        private string GetDefaultProfileImageUri() => Path.Combine("ProfileImage", "default.jpg");
    }
}