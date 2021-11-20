using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BlogApp.Data.FormModels
{
    public class CreateBlog
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Description { get; set; }
        [Display(Name = "Cover image")]
        public IFormFile CoverImage { get; set; }
    }
}