using System.Threading.Tasks;
using BlogApp.Models;

namespace BlogApp.Services
{
    public interface IUserModerationService
    {
        Task<bool> BanTicketExistsAsync(string userName);
        Task<BanTicket> FindAsync(string userName);
        Task HideCommentAsync(int commentId);
        Task HideBlogAsync(int blogId);
        Task RemoveAsync(BanTicket ticket);
    }
}