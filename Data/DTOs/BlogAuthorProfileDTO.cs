using System;
using System.Collections.Generic;
using System.Linq;
using BlogApp.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace BlogApp.Data.DTOs
{
    public class BlogAuthorProfileDto : BaseProfileDto
    {
        public string Country { get; set; } = "None";
        public string Description { get; set; } = "None";
        public static async Task<BlogAuthorProfileDto> From(
            UserManager<ApplicationUser> userManager,
            string userName)
        {
            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
                return new BlogAuthorProfileDto()
                {
                    Country = "Deleted",
                    Description = "Deleted",
                    UserName = userName
                };

            return new BlogAuthorProfileDto()
            {
                Country = user.Country,
                Description = user.Description,
                ProfilePicturePath = user.ProfilePicture,
                UserName = user.UserName
            };

        }
    }
}
