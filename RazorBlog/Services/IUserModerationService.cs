using System;
using System.Threading.Tasks;
using RazorBlog.Communication;
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
    Task<BanTicket?> FindBanTicketByUserNameAsync(string userName);

    /// <summary>
    ///     Remove the ban ticket immediately.
    /// </summary>
    /// <param name="bannedUserName">Name of banned user.</param>
    /// <param name="userName">Name of user lifting the ban.</param>
    /// <returns></returns>
    Task<ServiceResultCode> RemoveBanTicketAsync(string bannedUserName, string userName);

    /// <summary>
    ///     Create a ban ticket.
    /// </summary>
    /// <param name="userToBanName">Name of user being banned.</param>
    /// <param name="userName">Name of user executing the ban.</param>
    /// <param name="expiry">Expiry date of the ticket. Ban is permanent if null.</param>
    /// <returns></returns>
    Task<ServiceResultCode> BanUserAsync(string userToBanName, string userName, DateTime? expiry);
}