using RazorBlog.Communication;
using System.Threading.Tasks;

namespace RazorBlog.Services;
public interface IPostModerationService
{
    /// <summary>
    /// Hides a blog asynchronously.
    /// </summary>
    /// <param name="blogId">The identifier of the blog to be hidden.</param>
    /// <param name="name">The name of the user hiding the blog.</param>
    /// <returns>The result code indicating the outcome of the hide operation.</returns>
    Task<ServiceResultCode> HideBlogAsync(int blogId, string name);

    /// <summary>
    /// Hides a comment asynchronously.
    /// </summary>
    /// <param name="commentId">The identifier of the comment to be hidden.</param>
    /// <param name="name">The name of the user hiding the comment.</param>
    /// <returns>The result code indicating the outcome of the hide operation.</returns>
    Task<ServiceResultCode> HideCommentAsync(int commentId, string name);

    /// <summary>
    /// Forcibly deletes a comment asynchronously.
    /// </summary>
    /// <param name="commentId">The identifier of the comment to be forcibly deleted.</param>
    /// <param name="deletorName">The name of the user forcibly deleting the comment.</param>
    /// <returns>The result code indicating the outcome of the delete operation.</returns>
    Task<ServiceResultCode> ForciblyDeleteCommentAsync(int commentId, string deletorName);

    /// <summary>
    /// Forcibly deletes a blog asynchronously.
    /// </summary>
    /// <param name="commentId">The identifier of the blog to be forcibly deleted.</param>
    /// <param name="deletorName">The name of the user forcibly deleting the blog.</param>
    /// <returns>The result code indicating the outcome of the delete operation.</returns>
    Task<ServiceResultCode> ForciblyDeleteBlogAsync(int commentId, string deletorName);

    /// <summary>
    /// Unhides a blog asynchronously.
    /// </summary>
    /// <param name="blogId">The identifier of the blog to be unhidden.</param>
    /// <param name="name">The name of the user unhiding the blog.</param>
    /// <returns>The result code indicating the outcome of the unhide operation.</returns>
    Task<ServiceResultCode> UnhideBlogAsync(int blogId, string name);

    /// <summary>
    /// Unhides a comment asynchronously.
    /// </summary>
    /// <param name="commentId">The identifier of the comment to be unhidden.</param>
    /// <param name="name">The name of the user unhiding the comment.</param>
    /// <returns>The result code indicating the outcome of the unhide operation.</returns>
    Task<ServiceResultCode> UnhideCommentAsync(int commentId, string name);
}
