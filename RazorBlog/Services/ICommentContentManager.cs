using RazorBlog.Communication;
using RazorBlog.Data.ViewModels;
using System.Threading.Tasks;

namespace RazorBlog.Services;
public interface ICommentContentManager
{
    Task<(ServiceResultCode, int)> CreateCommentAsync(CommentViewModel createCommentViewModel, string userName);
    Task<ServiceResultCode> DeleteCommentAsync(int commentId, string userName);
    Task<ServiceResultCode> UpdateCommentAsync(int commentId, CommentViewModel editCommentViewModel, string userName);
}