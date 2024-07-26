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

    public async Task<IReadOnlyCollection<CommentDto>> GetCommentsAsync(int threadId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var comments = await dbContext.Comment
            .AsNoTracking()
            .Include(x => x.AuthorUser)
            .Where(x => x.ThreadId == threadId)
            .OrderByDescending(x => x.CreationTime)
            .ThenByDescending(x => x.LastUpdateTime)
            .ToListAsync();

        return await Task.WhenAll(comments
            .Select(async c => new CommentDto
            {
                Id = c.Id,
                CreationTime = c.CreationTime,
                LastUpdateTime = c.LastUpdateTime,
                Content = c.IsHidden ? ReplacementText.HiddenContent : c.Body,
                AuthorName = c.AuthorUser.UserName ?? ReplacementText.DeletedUser,
                AuthorProfileImageUri = await _aggregateImageUriResolver.ResolveImageUriAsync(c.AuthorUser.ProfileImageUri)
                    ?? await _defaultProfileImageProvider.GetDefaultProfileImageUriAsync(),
                IsHidden = c.IsHidden,
                IsDeleted = c.ToBeDeleted,
            })
            .ToList());
    }

    public async Task<(ServiceResultCode, IReadOnlyCollection<HiddenCommentDto>)> GetHiddenCommentsAsync(
        string authorUserName,
        string requestUserName)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (!await _userPermissionValidator.IsUserAllowedToReviewHiddenPostAsync(requestUserName))
        {
            return (ServiceResultCode.Unauthorized, []);
        }

        var hiddenComments = await dbContext.Comment
            .AsNoTracking()
            .Include(c => c.AuthorUser)
            .Where(c => c.AuthorUser.UserName == authorUserName && c.IsHidden)
            .Select(c => new HiddenCommentDto
            {
                Id = c.Id,
                Content = c.Body,
                CreationTime = c.CreationTime,
            })
            .ToListAsync();

        return (ServiceResultCode.Success, hiddenComments);
    }
}
