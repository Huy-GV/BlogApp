using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RazorBlog.Core.Data.Constants;
using RazorBlog.Core.Models;
using RazorBlog.Core.WriteServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorBlog.Core.ReadServices;

internal class UserPermissionValidator : IUserPermissionValidator
{
    private readonly IUserModerationService _userModerationService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserPermissionValidator> _logger;

    public UserPermissionValidator(
        IUserModerationService userModerationService,
        UserManager<ApplicationUser> userManager,
        ILogger<UserPermissionValidator> logger)
    {
        _logger = logger;
        _userManager = userManager;
        _userModerationService = userModerationService;
    }

    public async Task<bool> IsUserAllowedToHidePostAsync(string userName, string postAuthorUserName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return false;
        }

        if (user.UserName == postAuthorUserName)
        {
            return false;
        }

        if (await _userModerationService.BanTicketExistsAsync(user.UserName ?? string.Empty))
        {
            return false;
        }

        if (!await _userManager.IsInRoleAsync(user, Roles.ModeratorRole) &&
            !await _userManager.IsInRoleAsync(user, Roles.AdminRole))
        {
            return false;
        }

        var postAuthorUser = await _userManager.FindByNameAsync(postAuthorUserName);
        ArgumentNullException.ThrowIfNull(postAuthorUser);

        if (await _userManager.IsInRoleAsync(postAuthorUser, Roles.AdminRole))
        {
            _logger.LogError($"Posts authored by admin users cannot be hidden");
            return false;
        }

        return true;
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
