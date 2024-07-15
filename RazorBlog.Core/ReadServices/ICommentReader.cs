using RazorBlog.Core.Communication;
using RazorBlog.Core.Data.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RazorBlog.Core.ReadServices;
public interface ICommentReader
{
    Task<IReadOnlyCollection<CommentDto>> GetCommentsAsync(int blogId);
    Task<(ServiceResultCode, IReadOnlyCollection<HiddenCommentDto>)> GetHiddenCommentsAsync(
        string authorUserName,
        string requestUserName);
}
