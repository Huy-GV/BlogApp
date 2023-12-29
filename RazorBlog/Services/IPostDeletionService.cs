using System;

namespace RazorBlog.Services;

public interface IPostDeletionService
{
    public void ScheduleBlogDeletion(DateTimeOffset deleteTime, int blogId);
    public void ScheduleCommentDeletion(DateTimeOffset deleteTime, int commentId);
}
