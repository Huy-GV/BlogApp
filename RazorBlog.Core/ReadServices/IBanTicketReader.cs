using RazorBlog.Core.Models;
using System.Threading.Tasks;

namespace RazorBlog.Core.ReadServices;
public interface IBanTicketReader
{
    /// <summary>
    /// Checks if a user is banned.
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
}
