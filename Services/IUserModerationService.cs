using System;
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

    /// <summary>
    /// Find a ban ticket of a user via their user name.
    /// </summary>
    /// <param name="userName">Name of user with the ban ticket.</param>
    /// <returns>Ban ticket if it exists.</returns>
    Task<BanTicket?> FindAsync(string userName);

    /// <summary>
    ///     Marks the comment content as hidden until it is unhidden or deleted.
    /// </summary>
    /// <param name="commentId">ID of comment to hide.</param>
    /// <returns></returns>
    Task HideCommentAsync(int commentId);

    /// <summary>
    ///     Marks the blog title, description, and content as hidden until it is unhidden or deleted.
    /// </summary>
    /// <param name="blogId">ID of blog to hide.</param>
    /// <returns></returns>
    Task HideBlogAsync(int blogId);

    /// <summary>
    ///     Remove the ban ticket immediately.
    /// </summary>
    /// <param name="userName">Name of user being banned.</param>
    /// <returns></returns>
    Task RemoveBanTicketAsync(string userName);

    /// <summary>
    ///     Create a ban ticket.
    /// </summary>
    /// <param name="userName">Name of user being banned.</param>
    /// <param name="expiry">Expiry date of the ticket. Ban is permanent if null.</param>
    /// <returns></returns>
    Task BanUser(string userName, DateTime? expiry);
}