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
using SimpleForum.Core.QueryServices;

namespace SimpleForum.Core.CommandServices;

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
        comment.Body = ReplacementText.RemovedContent;
        comment.ToBeDeleted = true;
    }

    private static void CensorDeletedThread(Thread thread)
    {
        thread.ToBeDeleted = true;
        thread.Title = ReplacementText.RemovedContent;
        thread.Introduction = ReplacementText.RemovedContent;
        thread.Body = ReplacementText.RemovedContent;
    }

    public async Task<ServiceResultCode> ReportCommentAsync(int commentId, string requestUserName)
    {
        var comment = await _dbContext.Comment
            .Include(x => x.AuthorUser)
            .FirstOrDefaultAsync(x => x.Id == commentId);

        if (comment == null)
        {
            _logger.LogError("Comment with ID {commentId} not found", commentId);
            return ServiceResultCode.NotFound;
        }

        if (!await _userPermissionValidator.IsUserAllowedToReportPostAsync(requestUserName, comment.AuthorUserName))
        {
            _logger.LogError("Comment with ID {commentId} cannot be hidden by user {userName}", commentId, requestUserName);
            return ServiceResultCode.Unauthorized;
        }

        var reportTicket = new ReportTicket
        {
            CreationDate = DateTime.UtcNow,
            CommentId = commentId,
            ReportingUserName = requestUserName
        };

        _dbContext.ReportTicket.Add(reportTicket);
        comment.ReportTicket = reportTicket;
        await _dbContext.SaveChangesAsync();

        return ServiceResultCode.Success;
    }

    public async Task<ServiceResultCode> ReportThreadAsync(int threadId, string requestUserName)
    {
        var thread = await _dbContext.Thread
            .Include(x => x.AuthorUser)
            .FirstOrDefaultAsync(x => x.Id == threadId);

        if (thread == null)
        {
            _logger.LogError("Thread with ID {threadId} not found", threadId);
            return ServiceResultCode.NotFound;
        }

        if (!await _userPermissionValidator.IsUserAllowedToReportPostAsync(requestUserName, thread.AuthorUserName))
        {
            _logger.LogError(message: "Thread with ID {threadId} cannot be hidden by user {userName}", threadId, requestUserName);
            return ServiceResultCode.Unauthorized;
        }

        var reportTicket = new ReportTicket
        {
            CreationDate = DateTime.UtcNow,
            ThreadId = threadId,
            ReportingUserName = requestUserName
        };

        _dbContext.ReportTicket.Add(reportTicket);
        thread.ReportTicket = reportTicket;

        await _dbContext.SaveChangesAsync();

        return ServiceResultCode.Success;
    }

    public async Task<ServiceResultCode> CancelReportTicket(int reportTicketId, string userName)
    {
        var reportTicket = await _dbContext.ReportTicket
            .Include(x => x.ReportingUser)
            .FirstOrDefaultAsync(x => x.Id == reportTicketId);

        if (reportTicket == null)
        {
            _logger.LogError("Report ticket with ID {reportTicketId} not found", reportTicketId);
            return ServiceResultCode.NotFound;
        }

        if (!await IsUserInAdminRole(userName))
        {
            _logger.LogError(message: "Report ticket with ID {reportTicketId} cannot be cancelled by user {userName}", reportTicketId, userName);
            return ServiceResultCode.Unauthorized;
        }


        _dbContext.ReportTicket.Remove(reportTicket);

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
            .Include(x => x.ReportTicket)
            .FirstOrDefaultAsync(x => x.Id == commentId);

        if (comment == null)
        {
            return ServiceResultCode.NotFound;
        }

        if (comment.ReportTicketId == null)
        {
            _logger.LogError("Comment with ID {commentId} must be hidden before being forcibly deleted", commentId);
            return ServiceResultCode.Unauthorized;
        }


        if (await _featureManager.IsEnabledAsync(FeatureNames.UseHangFire))
        {
            CensorDeletedComment(comment);
            comment.ReportTicket!.ActionDate = DateTime.UtcNow;
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

        if (thread.ReportTicketId == null)
        {
            _logger.LogError("Thread with ID {threadId} must be reported before being forcibly deleted", threadId);
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
