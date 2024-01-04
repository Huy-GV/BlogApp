using RazorBlog.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RazorBlog.Services;
public interface IUserPermissionValidator
{
    /// <summary>
    /// Checks if the user is allowed to create a post asynchronously.
    /// </summary>
    /// <param name="userName">The name of the user to check permission for.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result is <c>true</c> if the user is allowed to create a post; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> IsUserAllowedToCreatePostAsync(string userName);

    /// <summary>
    /// Checks if the user is allowed to update or delete a specific post asynchronously.
    /// </summary>
    /// <typeparam name="TPostId">The type of the post identifier.</typeparam>
    /// <param name="userName">The name of the user to check permission for.</param>
    /// <param name="post">The post to check permission for.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result is <c>true</c> if the user is allowed to update or delete the specified post; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> IsUserAllowedToUpdateOrDeletePostAsync<TPostId>(string userName, Post<TPostId> post) where TPostId : notnull;

    /// <summary>
    /// Checks if the user is allowed to update or delete multiple posts asynchronously.
    /// </summary>
    /// <typeparam name="TPostId">The type of the post identifier.</typeparam>
    /// <param name="userName">The name of the user to check permission for.</param>
    /// <param name="posts">The collection of posts to check permission for.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result is a dictionary indicating, for each post, whether the user is allowed to update or delete it.
    /// </returns>
    Task<IReadOnlyDictionary<TPostId, bool>> IsUserAllowedToUpdateOrDeletePostsAsync<TPostId>(string userName, IEnumerable<Post<TPostId>> posts) where TPostId : notnull;
}
