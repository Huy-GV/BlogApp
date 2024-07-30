using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimpleForum.Core.Communication;
using SimpleForum.Core.Data;
using SimpleForum.Core.Data.Constants;
using SimpleForum.Core.Data.Dtos;

namespace SimpleForum.Core.QueryServices;

internal class ThreadReader : IThreadReader
{
    private readonly IDbContextFactory<SimpleForumDbContext> _dbContextFactory;
    private readonly IAggregateImageUriResolver _aggregateImageUriResolver;
    private readonly IDefaultProfileImageProvider _defaultProfileImageProvider;
    private readonly IUserPermissionValidator _userPermissionValidator;
    public ThreadReader(
        IDbContextFactory<SimpleForumDbContext> dbContextFactory,
        IAggregateImageUriResolver aggregateImageUriResolver,
        IUserPermissionValidator userPermissionValidator,
        IDefaultProfileImageProvider defaultProfileImageProvider)
    {
        _dbContextFactory = dbContextFactory;
        _aggregateImageUriResolver = aggregateImageUriResolver;
        _userPermissionValidator = userPermissionValidator;
        _defaultProfileImageProvider = defaultProfileImageProvider;
    }

    public async Task<IReadOnlyCollection<IndexThreadDto>> GetThreadsAsync(
        string? searchString = null,
        int page = 0,
        int pageSize = 10)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var threads = await dbContext.Thread
            .Include(x => x.AuthorUser)
            .Include(x => x.Comments)
            .ThenInclude(y => y.AuthorUser)
            .Where(x => x.ReportTicketId == null)
            .Select(x => new IndexThreadDto
            {
                Id = x.Id,
                Title = x.Title,
                AuthorName = x.AuthorUser.UserName!,
                CreationTime = x.CreationTime,
                LastUpdateTime = x.LastUpdateTime,
                CoverImageUri = x.CoverImageUri,
                Introduction = x.Introduction
            })
            .Where(x => string.IsNullOrEmpty(searchString) ||
                        x.Title.Contains(searchString) ||
                        x.AuthorName.Contains(searchString))
            .OrderByDescending(x => x.CreationTime)
            .ThenByDescending(x => x.LastUpdateTime)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return await Task.WhenAll(threads
            .Select(async x => x with
            {
                CoverImageUri = await _aggregateImageUriResolver.ResolveImageUriAsync(x.CoverImageUri) ?? string.Empty
            }).ToList());
    }

    public async Task<(ServiceResultCode, DetailedThreadDto?)> GetThreadAsync(int id, string requestUserName)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var thread = await dbContext.Thread
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(thread => thread.AuthorUser)
            .Include(thread => thread.ReportTicket)
            .FirstOrDefaultAsync(thread => thread.Id == id);

        if (thread == null)
        {
            return (ServiceResultCode.NotFound, null);
        }

        //thread.ViewCount++;
        //await _dbContext.SaveChangesAsync();

        var resolvedCoverImageUri = await _aggregateImageUriResolver.ResolveImageUriAsync(thread.CoverImageUri)
            ?? string.Empty;
        var resolvedAuthorProfileImageUri = await _aggregateImageUriResolver.ResolveImageUriAsync(
            thread.AuthorUser.ProfileImageUri)
            ?? _defaultProfileImageProvider.GetDefaultProfileImageUri();

        var isUserAllowedToViewHiddenPost = await _userPermissionValidator.IsUserAllowedToViewReportedPostAsync(requestUserName);
        var isThreadReported = thread.ReportTicketId != null;

        var threadDto = new DetailedThreadDto
        {
            Id = thread.Id,

            Introduction = !isThreadReported || isUserAllowedToViewHiddenPost
                ? thread.Introduction
                : ReplacementText.HiddenContent,
            Title = !isThreadReported || isUserAllowedToViewHiddenPost
                ? thread.Title
                : ReplacementText.HiddenContent,
            Content = !isThreadReported || isUserAllowedToViewHiddenPost
                ? thread.Body
                : ReplacementText.HiddenContent,
            CoverImageUri = resolvedCoverImageUri,
            CreationTime = thread.CreationTime,
            LastUpdateTime = thread.LastUpdateTime,
            AuthorDescription = thread.AuthorUser.Description,
            AuthorUserName = thread.AuthorUser.UserName ?? ReplacementText.DeletedUser,
            AuthorProfileImageUri = resolvedAuthorProfileImageUri,
            ReportTicket = isThreadReported
                ? new ReportTicketDto
                {
                    Id = thread.ReportTicketId!.Value,
                    ReportDate = thread.ReportTicket!.CreationDate,
                    ReportingUserName = thread.ReportTicket.ReportingUserName
                }
                : null
        };

        return (ServiceResultCode.Success, threadDto);
    }

    public async Task<(ServiceResultCode, IReadOnlyCollection<ReportedThreadDto>)> GetReportedThreadsAsync(
        string authorUserName,
        string requestUserName)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (!await _userPermissionValidator.IsUserAllowedToReviewReportedPostAsync(requestUserName))
        {
            return (ServiceResultCode.Unauthorized, []);
        }

        var hiddenThreads = await dbContext.Thread
            .AsNoTracking()
            .Include(x => x.AuthorUser)
            .Include(x => x.ReportTicket)
            .Where(x => x.AuthorUser.UserName == authorUserName 
                && x.ReportTicket != null
                && x.ReportTicket.CommentId == null)
            .Select(x => new ReportedThreadDto
            {
                Id = x.Id,
                Title = x.Title,
                CreationTime = x.CreationTime,
                ReportTicket = new ReportTicketDto
                {
                    Id = x.ReportTicketId!.Value,
                    ReportDate = x.ReportTicket!.CreationDate,
                    ReportingUserName = x.ReportTicket.ReportingUserName
                }
            })
            .ToListAsync();

        return (ServiceResultCode.Success, hiddenThreads);
    }
}
