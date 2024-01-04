using RazorBlog.Communication;
using RazorBlog.Data.ViewModels;
using System.Threading.Tasks;

namespace RazorBlog.Services;
public interface ICommentContentManager
{
    /// <summary>
    /// Creates a new comment asynchronously.
    /// </summary>
    /// <param name="createCommentViewModel">The view model containing information for creating a comment.</param>
    /// <param name="userName">The name of the user creating the comment.</param>
    /// <returns>
    /// A tuple containing the result code and the newly created comment's identifier.
    /// The result code indicates the outcome of the operation.
    /// </returns>
    Task<(ServiceResultCode, int)> CreateCommentAsync(CommentViewModel createCommentViewModel, string userName);

    /// <summary>
    /// Deletes a comment asynchronously.
    /// </summary>
    /// <param name="commentId">The identifier of the comment to be deleted.</param>
    /// <param name="userName">The name of the user attempting to delete the comment.</param>
    /// <returns>
    /// The result code indicating the outcome of the delete operation.
    /// </returns>
    Task<ServiceResultCode> DeleteCommentAsync(int commentId, string userName);

    /// <summary>
    /// Updates an existing comment asynchronously.
    /// </summary>
    /// <param name="commentId">The identifier of the comment to be updated.</param>
    /// <param name="editCommentViewModel">The view model containing updated information for the comment.</param>
    /// <param name="userName">The name of the user updating the comment.</param>
    /// <returns>
    /// The result code indicating the outcome of the update operation.
    /// </returns>
    Task<ServiceResultCode> UpdateCommentAsync(int commentId, CommentViewModel editCommentViewModel, string userName);
}
