using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SimpleForum.Core.Data;
using SimpleForum.Core.Data.Constants;
using SimpleForum.Core.Data.ViewModels;
using SimpleForum.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleForum.Core.ReadServices;

internal class UserPermissionValidator : IUserPermissionValidator
{
    private readonly IDbContextFactory<SimpleForumDbContext> _dbContextFactory;
    private readonly IBanTicketReader _banTicketReader;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserPermissionValidator> _logger;

    public UserPermissionValidator(
        IBanTicketReader banTicketReader,
        UserManager<ApplicationUser> userManager,
        ILogger<UserPermissionValidator> logger,
        IDbContextFactory<SimpleForumDbContext> dbContextFactory)
    {
        _logger = logger;
        _userManager = userManager;
        _banTicketReader = banTicketReader;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<bool> IsUserAllowedToReportPostAsync(string userName, string postAuthorUserName)
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

        if (await _banTicketReader.BanTicketExistsAsync(user.UserName ?? string.Empty))
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
        IEnumerable<PostPermissionViewModel<TPostId>> posts) where TPostId : notnull
    {
        var allowedToCreatePost = await IsUserAllowedToCreatePostAsync(userName);

        return posts.ToDictionary(
            x => x.PostId,
            x =>
                !string.IsNullOrWhiteSpace(x.AuthorUserName) &&
                userName == x.AuthorUserName &&
                x.ReportTicketId == null &&
                allowedToCreatePost);
    }

    public async Task<bool> IsUserAllowedToCreatePostAsync(string userName)
    {
        return !await _banTicketReader.BanTicketExistsAsync(userName);
    }

    public async Task<bool> IsUserAllowedToReviewReportedPostAsync(string userName)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var role = await dbContext.Roles
            .Where(r => r.Name == Roles.AdminRole)
            .FirstAsync();

        var user = await dbContext.Users
            .Select(x => new { x.UserName, x.Id})
            .FirstOrDefaultAsync(x => x.UserName == userName);

        if (user == null)
        {
            return false;
        }

        var userRole = await dbContext.UserRoles
            .Select(x => new { x.RoleId, x.UserId })
            .Where(x => x.UserId == user.Id && x.RoleId == role.Id)
            .FirstOrDefaultAsync();

        return userRole != null;
    }

    public async Task<bool> IsUserAllowedToViewReportedPostAsync(string userName)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var roleIds = dbContext.Roles
            .Where(r => r.Name == Roles.AdminRole || r.Name == Roles.ModeratorRole)
            .Select(x => x.Id);

        var user = await dbContext.Users
            .Select(x => new { x.UserName, x.Id})
            .FirstOrDefaultAsync(x => x.UserName == userName);

        if (user == null)
        {
            return false;
        }

        var userRole = await dbContext.UserRoles
            .Select(x => new { x.RoleId, x.UserId })
            .Where(x => x.UserId == user.Id && roleIds.Contains(x.RoleId))
            .FirstOrDefaultAsync();

        return userRole != null;
    }
}
