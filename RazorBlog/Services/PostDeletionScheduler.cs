using System;
using System.Linq;
using System.Linq.Expressions;
using Hangfire;
using Microsoft.Extensions.Logging;
using RazorBlog.Data;

namespace RazorBlog.Services;

public class PostDeletionScheduler : IPostDeletionScheduler
{
    private readonly RazorBlogDbContext _dbContext;
    private readonly ILogger<IPostDeletionScheduler> _logger;

    public PostDeletionScheduler(RazorBlogDbContext dbContext,
        ILogger<IPostDeletionScheduler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public void ScheduleBlogDeletion(DateTimeOffset deleteTime, int blogId)
    {
        _logger.LogInformation("Blog with ID {blogId} scheduled for deletion", blogId);
        Expression<Action> deleteBlog = () => DeleteBlog(blogId);
        BackgroundJob.Schedule(deleteBlog, deleteTime);
    }

    public void DeleteBlog(int blogId)
    {
        var blogToDelete = _dbContext.Blog.FirstOrDefault(x => x.Id == blogId);
        if (blogToDelete == null)
        {
            _logger.LogInformation("Blog with ID {blogId} already deleted ahead of schedule", blogId);
            return;
        }

        _dbContext.Blog.Remove(blogToDelete);
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
        Expression<Action> deleteComment = () => DeleteComment(commentId);
        BackgroundJob.Schedule(deleteComment, deleteTime);
    }
}
