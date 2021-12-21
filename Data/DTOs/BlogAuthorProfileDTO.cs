using System;
using System.Collections.Generic;
using System.Linq;
using BlogApp.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace BlogApp.Data.DTOs
{
    public class BlogAuthorProfileDTO : BaseProfileDTO
    {
        public string Country { get; set; } = "None";
        public string Description { get; set; } = "None";
        public static async Task<BlogAuthorProfileDTO> From(
            UserManager<ApplicationUser> userManager,
            string userName)
        {
            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
                return new BlogAuthorProfileDTO()
                {
                    Country = "Deleted",
                    Description = "Deleted",
                    Username = userName
                };

            return new BlogAuthorProfileDTO()
            {
                Country = user.Country,
                Description = user.Description,
                ProfilePath = user.ProfilePicture,
                Username = user.UserName
            };

        }
    }
}
