using RazorBlog.Communication;
using RazorBlog.Models;
using System.Threading.Tasks;

namespace RazorBlog.Services;
public interface IPostModerationService
{
    Task<BanTicket?> FindByUserNameAsync(string userName);
    Task<ServiceResultCode> HideBlogAsync(int blogId, string userName);
    Task<ServiceResultCode> HideCommentAsync(int commentId, string userName);
    Task<ServiceResultCode> ForciblyDeleteCommentAsync(int commentId, string deletorUserName);
    Task<ServiceResultCode> UnhideBlogAsync(int blogId, string userName);
    Task<ServiceResultCode> UnhideCommentAsync(int commentId, string userName);
}