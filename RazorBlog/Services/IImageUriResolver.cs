using System.Threading.Tasks;
using RazorBlog.Communication;

namespace RazorBlog.Services;

public interface IImageUriResolver
{
    public Task<(ServiceResultCode, string?)> ResolveImageUri(string imageUri);
}