using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RazorBlog.Communication;

namespace RazorBlog.Services;

public interface IImageStorage
{
    /// <summary>
    /// Uploads the cover image of a blog.
    /// </summary>
    /// <param name="imageFile">The image file to be uploaded.</param>
    /// <returns>A ServiceResultCode and the URI of the uploaded image if the result code is <see cref="ServiceResultCode.Success"/>.</returns>
    Task<(ServiceResultCode, string?)> UploadBlogCoverImageAsync(IFormFile imageFile);

    /// <summary>
    /// Uploads the profile image of a user.
    /// </summary>
    /// <param name="imageFile">The image file to be uploaded.</param>
    /// <returns>A ServiceResultCode and the URI of the uploaded image if the result code is <see cref="ServiceResultCode.Success"/>.</returns>
    Task<(ServiceResultCode, string?)> UploadProfileImageAsync(IFormFile imageFile);

    /// <summary>
    /// Gets the default profile image URI.
    /// </summary>
    /// <returns>The URI of the default profile image.</returns>
    Task<string> GetDefaultProfileImageUriAsync();
    
    /// <summary>
    /// Deletes an image identified by the provided URI. 
    /// This is used when a user wants to revert their profile image to default or upload a new profile/blog cover image.
    /// </summary>
    /// <param name="uri">The URI of the image to be deleted.</param>
    /// <returns>A ServiceResultCode indicating the success or failure of the operation.</returns>
    Task<ServiceResultCode> DeleteImage(string uri);
}
