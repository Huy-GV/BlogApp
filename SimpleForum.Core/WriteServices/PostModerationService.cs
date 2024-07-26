using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using SimpleForum.Core.Models;
using SimpleForum.Core.Communication;
using SimpleForum.Core.Data;
using SimpleForum.Core.Data.Constants;
using Microsoft.FeatureManagement;
using SimpleForum.Core.Features;
using SimpleForum.Core.ReadServices;

namespace SimpleForum.Core.WriteServices;

internal class PostModerationService : IPostModerationService
{
    private readonly SimpleForumDbContext _dbContext;
    private readonly ILogger<UserModerationService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPostDeletionScheduler _postDeletionScheduler;
    private readonly IUserPermissionValidator _userPermissionValidator;
    private readonly IFeatureManager _featureManager;

    public PostModerationService(
        SimpleForumDbContext dbContext,
        ILogger<UserModerationService> logger,
        UserManager<ApplicationUser> userManager,
        IPostDeletionScheduler postDeletionScheduler,
        IUserPermissionValidator userPermissionValidator,
        IFeatureManager featureManager)
    {
        _dbContext = dbContext;
        _logger = logger;
        _userManager = userManager;
        _postDeletionScheduler = postDeletionScheduler;
        _userPermissionValidator = userPermissionValidator;
        _featureManager = featureManager;
    }

    private async Task<bool> IsUserInAdminRole(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);

        return user != null && await _userManager.IsInRoleAsync(user, Roles.AdminRole);
    }

    private static void CensorDeletedComment(Comment comment)
    {
        comment.IsHidden = false;
        comment.Body = ReplacementText.RemovedContent;
        comment.ToBeDeleted = true;
    }

    private static void CensorDeletedThread(Thread thread)
    {
        thread.IsHidden = false;
        thread.ToBeDeleted = true;
        thread.Title = ReplacementText.RemovedContent;
        thread.Introduction = ReplacementText.RemovedContent;
        thread.Body = ReplacementText.RemovedContent;
    }

    public async Task<ServiceResultCode> HideCommentAsync(int commentId, string userName)
    {
        var comment = await _dbContext.Comment
            .Include(x => x.AuthorUser)
            .FirstOrDefaultAsync(x => x.Id == commentId);

        if (comment == null)
        {
            _logger.LogError("Comment with ID {commentId} not found", commentId);
            return ServiceResultCode.NotFound;
        }

        if (!await _userPermissionValidator.IsUserAllowedToHidePostAsync(userName, comment.AuthorUserName))
        {
            _logger.LogError("Comment with ID {commentId} cannot be hidden by user {userName}", commentId, userName);
            return ServiceResultCode.Unauthorized;
        }

        comment.IsHidden = true;
        await _dbContext.SaveChangesAsync();

        return ServiceResultCode.Success;
    }

    public async Task<ServiceResultCode> HideThreadAsync(int threadId, string userName)
    {
        var thread = await _dbContext.Thread
            .Include(x => x.AuthorUser)
            .FirstOrDefaultAsync(x => x.Id == threadId);

        if (thread == null)
        {
            _logger.LogError("Thread with ID {threadId} not found", threadId);
            return ServiceResultCode.NotFound;
        }

        if (!await _userPermissionValidator.IsUserAllowedToHidePostAsync(userName, thread.AuthorUserName))
        {
            _logger.LogError(message: "Thread with ID {threadId} cannot be hidden by user {userName}", threadId, userName);
            return ServiceResultCode.Unauthorized;
        }

        thread.IsHidden = true;
        await _dbContext.SaveChangesAsync();

        return ServiceResultCode.Success;
    }

    public async Task<ServiceResultCode> UnhideCommentAsync(int commentId, string userName)
    {
        var comment = await _dbContext.Comment
            .Include(x => x.AuthorUser)
            .FirstOrDefaultAsync(x => x.Id == commentId);

        if (comment == null)
        {
            _logger.LogError("Comment with ID {commentId} not found", commentId);
            return ServiceResultCode.NotFound;
        }

        if (!await IsUserInAdminRole(userName))
        {
            _logger.LogError("Comment with ID {commentId} cannot be un-hidden by user {userName}", commentId, userName);
            return ServiceResultCode.Unauthorized;
        }

        comment.IsHidden = false;
        await _dbContext.SaveChangesAsync();

        return ServiceResultCode.Success;
    }

    public async Task<ServiceResultCode> UnhideThreadAsync(int threadId, string userName)
    {
        var thread = await _dbContext.Thread
            .Include(x => x.AuthorUser)
            .FirstOrDefaultAsync(x => x.Id == threadId);

        if (thread == null)
        {
            _logger.LogError("thread with ID {threadId} not found", threadId);
            return ServiceResultCode.NotFound;
        }

        if (!await IsUserInAdminRole(userName))
        {
            _logger.LogError(message: "Thread with ID {threadId} cannot be un-hidden by user {userName}", threadId, userName);
            return ServiceResultCode.Unauthorized;
        }

        thread.IsHidden = false;
        await _dbContext.SaveChangesAsync();

        return ServiceResultCode.Success;
    }

    public async Task<ServiceResultCode> ForciblyDeleteCommentAsync(int commentId, string deletorUserName)
    {
        if (!await IsUserInAdminRole(deletorUserName))
        {
            return ServiceResultCode.Unauthorized;
        }

        var comment = await _dbContext.Comment
            .Include(x => x.AuthorUser)
            .FirstOrDefaultAsync(x => x.Id == commentId);

        if (comment == null)
        {
            return ServiceResultCode.NotFound;
        }

        if (!comment.IsHidden)
        {
            _logger.LogError("Comment with ID {commentId} must be hidden before being forcibly deleted", commentId);
            return ServiceResultCode.Unauthorized;
        }


        if (await _featureManager.IsEnabledAsync(FeatureNames.UseHangFire))
        {
            CensorDeletedComment(comment);
            _postDeletionScheduler.ScheduleCommentDeletion(
                new DateTimeOffset(DateTime.UtcNow.AddDays(7)),
                commentId);
        }
        else
        {
            _dbContext.Comment.Remove(comment);
        }

        await _dbContext.SaveChangesAsync();
        return ServiceResultCode.Success;
    }

    public async Task<ServiceResultCode> ForciblyDeleteThreadAsync(int threadId, string deletorUserName)
    {
        if (!await IsUserInAdminRole(deletorUserName))
        {
            return ServiceResultCode.Unauthorized;
        }

        var thread = await _dbContext.Thread
            .Include(x => x.AuthorUser)
            .FirstOrDefaultAsync(x => x.Id == threadId);

        if (thread == null)
        {
            return ServiceResultCode.NotFound;
        }

        if (!thread.IsHidden)
        {
            _logger.LogError("Thread with ID {threadId} must be hidden before being forcibly deleted", threadId);
            return ServiceResultCode.Unauthorized;
        }

        if (await _featureManager.IsEnabledAsync(FeatureNames.UseHangFire))
        {
            CensorDeletedThread(thread);
            _postDeletionScheduler.ScheduleThreadDeletion(
                new DateTimeOffset(DateTime.UtcNow.AddDays(7)),
                threadId);
        }
        else
        {
            _dbContext.Thread.Remove(thread);
        }

        await _dbContext.SaveChangesAsync();
        return ServiceResultCode.Success;
    }
}
