using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace BlogApp.Interfaces
{
    [Obsolete]
    public interface IImageService
    {
        Task UploadBlogImageAsync(IFormFile imageFile, string fileName);

        Task UploadProfileImageAsync(IFormFile imageFile, string fileName);

        void DeleteImage(string fileName);

        string BuildFileName(string originalName);
    }
}