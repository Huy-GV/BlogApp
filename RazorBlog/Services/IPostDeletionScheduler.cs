using System;

namespace RazorBlog.Services;

public interface IPostDeletionScheduler
{
    void ScheduleBlogDeletion(DateTimeOffset deleteTime, int blogId);
    void ScheduleCommentDeletion(DateTimeOffset deleteTime, int commentId);
}
