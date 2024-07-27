using SimpleForum.Core.Data.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleForum.Core.ReadServices;
public interface IUserPermissionValidator
{
    /// <summary>
    /// Checks if the user is allowed to create a post asynchronously.
    /// </summary>
    /// <param name="userName">The name of the user to check permission for.</param>
    /// <returns>
    /// The task result is <c>true</c> if the user is allowed to create a post; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> IsUserAllowedToCreatePostAsync(string userName);

    /// <summary>
    /// Checks if the user is allowed to hide a post.
    /// </summary>
    /// <param name="userName">The name of the user to check permission for.</param>
    /// <param name="postAuthorUserName">The name of the post author user to check permission for.</param>
    /// <returns>
    /// The task result is <c>true</c> if the user is allowed to hide a post; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> IsUserAllowedToReportPostAsync(string userName, string postAuthorUserName);

    /// <summary>
    /// Checks if the user is allowed to review a reported post.
    /// </summary>
    /// <param name="userName">The name of the user to check permission for.</param>
    /// <param name="authorUserName">The name of the author of reported posts.</param>
    /// <returns>
    /// The task result is <c>true</c> if the user is allowed to review reported posts; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> IsUserAllowedToReviewReportedPostAsync(string userName);

    /// <summary>
    /// Checks if the user is allowed to view a reported post.
    /// </summary>
    /// <param name="userName">The name of the user to check permission for.</param>
    /// <param name="authorUserName">The name of the author of reported posts.</param>
    /// <returns>
    /// The task result is <c>true</c> if the user is allowed to review reported posts; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> IsUserAllowedToViewReportedPostAsync(string userName);

    /// <summary>
    /// Checks if the user is allowed to update or delete a specific post asynchronously.
    /// </summary>
    /// <param name="userName">The name of the user to check permission for.</param>
    /// <param name="isPostReported">Whether the post is reported by a moderator.</param>
    /// <param name="postAuthorUsername">User name of the author.</param>
    /// <returns>
    /// The task result is <c>true</c> if the user is allowed to update or delete the specified post; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> IsUserAllowedToUpdateOrDeletePostAsync(string userName, bool isPostReported, string postAuthorUsername);

    /// <summary>
    /// Checks if the user is allowed to update or delete multiple posts asynchronously.
    /// </summary>
    /// <typeparam name="TPostId">The type of the post identifier.</typeparam>
    /// <param name="userName">The name of the user to check permission for.</param>
    /// <param name="posts">The collection of posts to check permission for.</param>
    /// <returns>
    /// The task result is a dictionary indicating, for each post, whether the user is allowed to update or delete it.
    /// </returns>
    Task<IReadOnlyDictionary<TPostId, bool>> IsUserAllowedToUpdateOrDeletePostsAsync<TPostId>(
        string userName,
         IEnumerable<PostPermissionViewModel<TPostId>> posts) where TPostId : notnull;
}
