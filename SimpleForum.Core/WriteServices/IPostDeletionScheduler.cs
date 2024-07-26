using System;

namespace SimpleForum.Core.WriteServices;

public interface IPostDeletionScheduler
{
    /// <summary>
    /// Schedules the deletion of a thread for a specific time.
    /// </summary>
    /// <param name="deleteTime">The date and time when the thread deletion is scheduled to occur.</param>
    /// <param name="threadId">The identifier of the thread to be deleted.</param>
    void ScheduleThreadDeletion(DateTimeOffset deleteTime, int threadId);

    /// <summary>
    /// Schedules the deletion of a comment for a specific time.
    /// </summary>
    /// <param name="deleteTime">The date and time when the comment deletion is scheduled to occur.</param>
    /// <param name="commentId">The identifier of the comment to be deleted.</param>
    void ScheduleCommentDeletion(DateTimeOffset deleteTime, int commentId);
}
