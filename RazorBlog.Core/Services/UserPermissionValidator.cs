using RazorBlog.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorBlog.Core.Services;

internal class UserPermissionValidator : IUserPermissionValidator
{
    private readonly IUserModerationService _userModerationService;

    public UserPermissionValidator(IUserModerationService userModerationService)
    {
        _userModerationService = userModerationService;
    }

    public async Task<bool> IsUserAllowedToUpdateOrDeletePostAsync(string userName, bool isPostHidden, string postAuthorUsername)
    {
        return
            !string.IsNullOrWhiteSpace(postAuthorUsername) &&
            userName == postAuthorUsername &&
            !isPostHidden &&
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
