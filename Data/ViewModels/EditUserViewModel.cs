using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BlogApp.Data.ViewModel
{
    public class EditUserViewModel
    {
        public string UserName { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string Country { get; set; }
        [Required]
        [StringLength(350, MinimumLength = 20)]
        public string Description { get; set; }
        public IFormFile ProfilePicture { get; set; }
    }
}