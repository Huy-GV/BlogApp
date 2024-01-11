using RazorBlog.Communication;
using RazorBlog.Data.ViewModels;
using System.Threading.Tasks;

namespace RazorBlog.Services;
public interface IBlogContentManager
{
    /// <summary>
    /// Creates a new blog asynchronously.
    /// </summary>
    /// <param name="createBlogViewModel">The view model containing information for creating a blog.</param>
    /// <param name="userName">The name of the user creating the blog.</param>
    /// <returns>
    /// A tuple containing the result code and the newly created blog's identifier.
    /// The result code indicates the outcome of the operation.
    /// </returns>
    Task<(ServiceResultCode, int?)> CreateBlogAsync(CreateBlogViewModel createBlogViewModel, string userName);

    /// <summary>
    /// Deletes a blog asynchronously.
    /// </summary>
    /// <param name="blogId">The identifier of the blog to be deleted.</param>
    /// <param name="userName">The name of the user attempting to delete the blog.</param>
    /// <returns>
    /// The result code indicating the outcome of the delete operation.
    /// </returns>
    Task<ServiceResultCode> DeleteBlogAsync(int blogId, string userName);

    /// <summary>
    /// Updates an existing blog asynchronously.
    /// </summary>
    /// <param name="editBlogViewModel">The view model containing updated information for the blog.</param>
    /// <param name="userName">The name of the user updating the blog.</param>
    /// <returns>
    /// The result code indicating the outcome of the update operation.
    /// </returns>
    Task<ServiceResultCode> UpdateBlog(EditBlogViewModel editBlogViewModel, string userName);
}
