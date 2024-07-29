using SimpleForum.Core.Communication;
using SimpleForum.Core.Data.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleForum.Core.QueryServices;
public interface ICommentReader
{
    Task<IReadOnlyCollection<CommentDto>> GetCommentsAsync(
        int threadId,
        string requestUserName);
    Task<(ServiceResultCode, IReadOnlyCollection<ReportedCommentDto>)> GetReportedCommentsAsync(
        string authorUserName,
        string requestUserName);
}
