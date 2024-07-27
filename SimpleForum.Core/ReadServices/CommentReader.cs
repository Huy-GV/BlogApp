using Microsoft.EntityFrameworkCore;
using SimpleForum.Core.Communication;
using SimpleForum.Core.Data;
using SimpleForum.Core.Data.Constants;
using SimpleForum.Core.Data.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleForum.Core.ReadServices;
internal class CommentReader : ICommentReader
{
    private readonly IDbContextFactory<SimpleForumDbContext> _dbContextFactory;
    private readonly IUserPermissionValidator _userPermissionValidator;
    private readonly IAggregateImageUriResolver _aggregateImageUriResolver;
    private readonly IDefaultProfileImageProvider _defaultProfileImageProvider;

    public CommentReader(
        IDbContextFactory<SimpleForumDbContext> dbContextFactory,
        IUserPermissionValidator userPermissionValidator,
        IAggregateImageUriResolver aggregateImageUriResolver,
        IDefaultProfileImageProvider defaultProfileImageProvider)
    {
        _dbContextFactory = dbContextFactory;
        _userPermissionValidator = userPermissionValidator;
        _aggregateImageUriResolver = aggregateImageUriResolver;
        _defaultProfileImageProvider = defaultProfileImageProvider;
    }

    public async Task<IReadOnlyCollection<CommentDto>> GetCommentsAsync(int threadId, string requestUserName)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var comments = await dbContext.Comment
            .AsNoTracking()
            .Include(x => x.AuthorUser)
            .Include(x => x.ReportTicket)
            .Where(x => x.ThreadId == threadId)
            .OrderByDescending(x => x.CreationTime)
            .ThenByDescending(x => x.LastUpdateTime)
            .ToListAsync();

        var isUserAllowedToViewHiddenPost = await _userPermissionValidator.IsUserAllowedToViewReportedPostAsync(requestUserName);

        return await Task.WhenAll(comments
            .Select(async x => new CommentDto
            {
                Id = x.Id,
                CreationTime = x.CreationTime,
                LastUpdateTime = x.LastUpdateTime,
                Content = (x.ReportTicketId == null || isUserAllowedToViewHiddenPost) ? x.Body : ReplacementText.HiddenContent,
                AuthorName = x.AuthorUser.UserName ?? ReplacementText.DeletedUser,
                AuthorProfileImageUri = await _aggregateImageUriResolver.ResolveImageUriAsync(x.AuthorUser.ProfileImageUri)
                    ?? await _defaultProfileImageProvider.GetDefaultProfileImageUriAsync(),
                IsDeleted = x.ToBeDeleted,
                ReportTicketId = x.ReportTicketId,
                ReportDate = x.ReportTicket?.CreationDate,
                ReportingUserName = x.ReportTicket?.ReportingUserName ?? string.Empty,
            })
            .ToList());
    }

    public async Task<(ServiceResultCode, IReadOnlyCollection<HiddenCommentDto>)> GetReportedCommentsAsync(
        string authorUserName,
        string requestUserName)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (!await _userPermissionValidator.IsUserAllowedToReviewReportedPostAsync(requestUserName))
        {
            return (ServiceResultCode.Unauthorized, []);
        }

        var reportedComments = await dbContext.Comment
            .AsNoTracking()
            .Include(c => c.AuthorUser)
            .Include(x => x.ReportTicket)
            .Where(c => c.AuthorUser.UserName == authorUserName && c.ReportTicketId != null && c.ReportTicket!.ActionDate == null)
            .Select(c => new HiddenCommentDto
            {
                Id = c.Id,
                CreationTime = c.CreationTime,
                ReportDate = c.ReportTicket!.CreationDate,
                ReportingUserName = c.ReportTicket.ReportingUserName,
                ThreadId = c.ThreadId,
                ReportTicketId = c.ReportTicketId!.Value
            })
            .ToListAsync();

        return (ServiceResultCode.Success, reportedComments);
    }
}
