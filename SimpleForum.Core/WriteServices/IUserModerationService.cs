using System;
using System.Threading.Tasks;
using SimpleForum.Core.Communication;

namespace SimpleForum.Core.WriteServices;

public interface IUserModerationService
{
    /// <summary>
    /// Remove the ban ticket immediately.
    /// </summary>
    /// <param name="bannedUserName">Name of banned user.</param>
    /// <param name="userName">Name of user lifting the ban.</param>
    /// <returns></returns>
    Task<ServiceResultCode> RemoveBanTicketAsync(string bannedUserName, string userName);

    /// <summary>
    /// Create a ban ticket.
    /// </summary>
    /// <param name="userToBanName">Name of user being banned.</param>
    /// <param name="userName">Name of user executing the ban.</param>
    /// <param name="expiry">Expiry date of the ticket. Ban is permanent if null.</param>
    /// <returns></returns>
    Task<ServiceResultCode> BanUserAsync(string userToBanName, string userName, DateTime? expiry);
}
