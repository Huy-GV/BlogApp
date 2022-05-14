using System.Threading.Tasks;
using RazorBlog.Models;

namespace RazorBlog.Services;

public interface IUserModerationService
{
    /// <summary>
    ///     Checks if a user is banned.
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    Task<bool> BanTicketExistsAsync(string userName);
    Task<BanTicket> FindAsync(string userName);
    /// <summary>
    ///     Marks the comment as [removed] until it is unhidden or deleted.
    /// </summary>
    /// <param name="commentId"></param>
    /// <returns></returns>
    Task HideCommentAsync(int commentId);
    /// <summary>
    ///     Marks the blog as [removed] until it is unhidden or deleted.
    /// </summary>
    /// <param name="blogId"></param>
    /// <returns></returns>
    Task HideBlogAsync(int blogId);
    Task RemoveBanTicketAsync(BanTicket ticket);
}