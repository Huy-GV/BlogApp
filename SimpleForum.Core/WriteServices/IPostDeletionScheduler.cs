using System;

namespace SimpleForum.Core.WriteServices;

public interface IPostDeletionScheduler
{
    /// <summary>
    /// Schedules the deletion of a blog for a specific time.
    /// </summary>
    /// <param name="deleteTime">The date and time when the blog deletion is scheduled to occur.</param>
    /// <param name="blogId">The identifier of the blog to be deleted.</param>
    void ScheduleBlogDeletion(DateTimeOffset deleteTime, int blogId);

    /// <summary>
    /// Schedules the deletion of a comment for a specific time.
    /// </summary>
    /// <param name="deleteTime">The date and time when the comment deletion is scheduled to occur.</param>
    /// <param name="commentId">The identifier of the comment to be deleted.</param>
    void ScheduleCommentDeletion(DateTimeOffset deleteTime, int commentId);
}
