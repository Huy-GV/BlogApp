using System.Collections.Generic;
using System.Threading.Tasks;
using RazorBlog.Communication;
using RazorBlog.Data.Dtos;

namespace RazorBlog.Services;

public interface IBlogReader
{
    Task<IReadOnlyCollection<IndexBlogDto>> GetBlogsAsync(
        string? searchString = null, 
        int page = 0, 
        int pageSize = 10);

    Task<(ServiceResultCode, DetailedBlogDto?)> GetBlogAsync(int id);
}