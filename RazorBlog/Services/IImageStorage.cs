using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RazorBlog.Services;

public interface IImageStorage
{
    /// <summary>
    /// Upload the cover image of a blog.
    /// </summary>
    /// <param name="imageFile"></param>
    /// <returns>The name of the uploaded image.</returns>
    Task<string> UploadBlogCoverImageAsync(IFormFile imageFile);

    /// <summary>
    /// Upload the profile image of a user.
    /// </summary>
    /// <param name="imageFile"></param>
    /// <returns>The name of the uploaded image.</returns>
    Task<string> UploadProfileImageAsync(IFormFile imageFile);

    /// <summary>
    /// Get the default profile image.
    /// </summary>
    /// <returns>The uri of default profile image.</returns>
    Task<string> GetDefaultProfileImageUriAsync();
    
    /// <summary>
    /// Called when user wants to revert their profile image to default or upload a new profile/ blog cover image
    /// </summary>
    Task DeleteImage(string uri);
}