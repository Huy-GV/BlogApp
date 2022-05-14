using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RazorBlog.Data.ViewModels;

public class EditBlogViewModel : CreateBlogViewModel
{
    public int Id { get; set; }

    [Display(Name = "Change cover image")] public new IFormFile CoverImage { get; set; }
}