using System.Threading.Tasks;

namespace RazorBlog.Services;

public interface IAggregateImageUriResolver
{
    public Task<string?> ResolveImageUriAsync(string imageUri);
}