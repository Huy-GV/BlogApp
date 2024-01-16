using RazorBlog.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorBlog.Services;

public class UserPermissionValidator : IUserPermissionValidator
{
    private readonly IUserModerationService _userModerationService;

    public UserPermissionValidator(IUserModerationService userModerationService)
    {
        _userModerationService = userModerationService;
    }

    public async Task<bool> IsUserAllowedToUpdateOrDeletePostAsync<TPostId>(string userName, Post<TPostId> post) where TPostId : notnull
    {
        return
            !string.IsNullOrWhiteSpace(post.AuthorUser.UserName) &&
            userName == post.AuthorUser.UserName &&
            !post.IsHidden &&
            await IsUserAllowedToCreatePostAsync(userName);
    }

    public async Task<IReadOnlyDictionary<TPostId, bool>> IsUserAllowedToUpdateOrDeletePostsAsync<TPostId>(
        string userName,
        IEnumerable<Post<TPostId>> posts) where TPostId : notnull
    {
        var allowedToCreatePost = await IsUserAllowedToCreatePostAsync(userName);

        return posts.ToDictionary(
            x => x.Id,
            x =>
                !string.IsNullOrWhiteSpace(x.AuthorUser.UserName) &&
                userName == x.AuthorUser.UserName &&
                !x.IsHidden &&
                allowedToCreatePost);
    }

    public async Task<bool> IsUserAllowedToCreatePostAsync(string userName)
    {
        return !await _userModerationService.BanTicketExistsAsync(userName);
    }
}
