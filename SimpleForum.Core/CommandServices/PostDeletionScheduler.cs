using System;
using System.Linq;
using Hangfire;
using Microsoft.Extensions.Logging;
using SimpleForum.Core.Data;

namespace SimpleForum.Core.CommandServices;

internal class PostDeletionScheduler : IPostDeletionScheduler
{
    private readonly SimpleForumDbContext _dbContext;
    private readonly ILogger<IPostDeletionScheduler> _logger;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public PostDeletionScheduler(
        SimpleForumDbContext dbContext,
        ILogger<IPostDeletionScheduler> logger,
        IBackgroundJobClient backgroundJobClient)
    {
        _dbContext = dbContext;
        _logger = logger;
        _backgroundJobClient = backgroundJobClient;
    }

    public void ScheduleThreadDeletion(DateTimeOffset deleteTime, int threadId)
    {
        _logger.LogInformation("Thread with ID {threadId} scheduled for deletion", threadId);
        _backgroundJobClient.Schedule(() => DeleteThread(threadId), deleteTime);
    }

    public void DeleteThread(int threadId)
    {
        var threadToDelete = _dbContext.Thread.FirstOrDefault(x => x.Id == threadId);
        if (threadToDelete == null)
        {
            _logger.LogInformation("Thread with ID {threadId} already deleted ahead of schedule", threadId);
            return;
        }

        _dbContext.Thread.Remove(threadToDelete);
        _dbContext.SaveChanges();
    }

    public void DeleteComment(int commentId)
    {
        _logger.LogInformation("Comment with ID {commentId} scheduled for deletion", commentId);
        var commentToDelete = _dbContext.Comment.FirstOrDefault(x => x.Id == commentId);
        if (commentToDelete == null)
        {
            _logger.LogInformation("Comment with ID {commentId} already deleted ahead of schedule", commentId);
            return;
        }

        _dbContext.Comment.Remove(commentToDelete);
        _dbContext.SaveChanges();
    }

    public void ScheduleCommentDeletion(DateTimeOffset deleteTime, int commentId)
    {
        _backgroundJobClient.Schedule(() => DeleteComment(commentId), deleteTime);
    }
}
