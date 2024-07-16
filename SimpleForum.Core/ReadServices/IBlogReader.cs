using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleForum.Core.Communication;
using SimpleForum.Core.Data.Dtos;

namespace SimpleForum.Core.ReadServices;

public interface IBlogReader
{
    /// <summary>
    /// Retrieves a collection of blog entries based on specified criteria asynchronously.
    /// </summary>
    /// <param name="searchString">Optional. The search string to filter blog entries.</param>
    /// <param name="page">Optional. The page number for paginated results (default is 0).</param>
    /// <param name="pageSize">Optional. The number of entries per page (default is 10).</param>
    /// <returns>
    /// The task result contains a read-only collection of <see cref="IndexBlogDto"/> representing the retrieved blog entries.
    /// </returns>
    Task<IReadOnlyCollection<IndexBlogDto>> GetBlogsAsync(
        string? searchString = null,
        int page = 0,
        int pageSize = 10);

    /// <summary>
    /// Retrieves a detailed blog entry based on its identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the blog entry to retrieve.</param>
    /// <returns>
    /// The task result contains a tuple with
    /// a <see cref="ServiceResultCode"/> indicating the result of the operation,
    /// and a <see cref="DetailedBlogDto"/> representing the detailed blog entry (null if not found).
    /// </returns>
    Task<(ServiceResultCode, DetailedBlogDto?)> GetBlogAsync(int id);

    /// <summary>
    /// Retrieves a detailed blog entry based on its identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the blog entry to retrieve.</param>
    /// <returns>
    /// The task result contains a tuple with
    /// a <see cref="ServiceResultCode"/> indicating the result of the operation,
    /// and a <see cref="DetailedBlogDto"/> representing the detailed blog entry (null if not found).
    /// </returns>
    Task<(ServiceResultCode, IReadOnlyCollection<HiddenBlogDto>)> GetHiddenBlogsAsync(
        string authorUserName,
        string requestUserName);
}
