using RazorBlog.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RazorBlog.Services;
public interface IUserPermissionValidator
{
    Task<bool> IsUserAllowedToCreatePostAsync(string userName);
    Task<bool> IsUserAllowedToUpdateOrDeletePostAsync<TPostId>(string userName, Post<TPostId> post) where TPostId : notnull;
    Task<IReadOnlyDictionary<TPostId, bool>> IsUserAllowedToUpdateOrDeletePostsAsync<TPostId>(string userName, IEnumerable<Post<TPostId>> posts) where TPostId : notnull;
}