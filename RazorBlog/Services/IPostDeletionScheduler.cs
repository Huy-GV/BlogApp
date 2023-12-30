using System;

namespace RazorBlog.Services;

public interface IPostDeletionScheduler
{
    public void ScheduleBlogDeletion(DateTimeOffset deleteTime, int blogId);
    public void ScheduleCommentDeletion(DateTimeOffset deleteTime, int commentId);
}
