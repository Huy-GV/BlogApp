using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimpleForum.Core.Communication;
using SimpleForum.Core.Data;
using SimpleForum.Core.Data.Constants;
using SimpleForum.Core.Data.Dtos;

namespace SimpleForum.Core.ReadServices;

internal class ThreadReader : IThreadReader
{
    private readonly IDbContextFactory<SimpleForumDbContext> _dbContextFactory;
    private readonly IAggregateImageUriResolver _aggregateImageUriResolver;
    private readonly IUserPermissionValidator _userPermissionValidator;
    public ThreadReader(
        IDbContextFactory<SimpleForumDbContext> dbContextFactory,
        IAggregateImageUriResolver aggregateImageUriResolver,
        IUserPermissionValidator userPermissionValidator)
    {
        _dbContextFactory = dbContextFactory;
        _aggregateImageUriResolver = aggregateImageUriResolver;
        _userPermissionValidator = userPermissionValidator;
    }

    public async Task<IReadOnlyCollection<IndexThreadDto>> GetThreadsAsync(
        string? searchString = null,
        int page = 0,
        int pageSize = 10)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var threads = await dbContext.Thread
            .Include(b => b.AuthorUser)
            .Include(b => b.Comments)
            .ThenInclude(c => c.AuthorUser)
            .Where(x => !x.IsHidden)
            .Select(b => new IndexThreadDto
            {
                Id = b.Id,
                Title = b.IsHidden ? ReplacementText.HiddenContent : b.Title,
                AuthorName = b.AuthorUser.UserName!,
                CreationTime = b.CreationTime,
                LastUpdateTime = b.LastUpdateTime,
                ViewCount = b.ViewCount,
                CoverImageUri = b.CoverImageUri,
                Introduction = b.IsHidden ? ReplacementText.HiddenContent : b.Introduction
            })
            .Where(b => string.IsNullOrEmpty(searchString) ||
                        b.Title.Contains(searchString) ||
                        b.AuthorName.Contains(searchString))
            .OrderByDescending(x => x.CreationTime)
            .ThenByDescending(x => x.LastUpdateTime)
            .Take(10)
            .ToListAsync();

        return await Task.WhenAll(threads
            .Select(async x =>
            {
                x.CoverImageUri = await _aggregateImageUriResolver.ResolveImageUriAsync(x.CoverImageUri) ?? string.Empty;
                return x;
            }).ToList());
    }

    public async Task<(ServiceResultCode, DetailedThreadDto?)> GetThreadAsync(int id)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var thread = await dbContext.Thread
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(thread => thread.AuthorUser)
            .FirstOrDefaultAsync(thread => thread.Id == id);

        if (thread == null)
        {
            return (ServiceResultCode.NotFound, null);
        }

        //thread.ViewCount++;
        //await _dbContext.SaveChangesAsync();

        var resolvedCoverImageUri = await _aggregateImageUriResolver.ResolveImageUriAsync(thread.CoverImageUri)
            ?? string.Empty;
        var resolvedAuthorProfileImageUri = await _aggregateImageUriResolver.ResolveImageUriAsync(thread.AuthorUser.ProfileImageUri)
            ?? string.Empty;

        var threadDto = new DetailedThreadDto
        {
            Id = thread.Id,
            Introduction = thread.IsHidden ? ReplacementText.HiddenContent : thread.Introduction,
            Title = thread.IsHidden ? ReplacementText.HiddenContent : thread.Title,
            Content = thread.IsHidden ? ReplacementText.HiddenContent : thread.Body,
            CoverImageUri = resolvedCoverImageUri,
            CreationTime = thread.CreationTime,
            LastUpdateTime = thread.LastUpdateTime,
            IsHidden = thread.IsHidden,
            AuthorDescription = thread.AuthorUser.Description,
            AuthorName = thread.AuthorUser.UserName ?? ReplacementText.DeletedUser,
            AuthorProfileImageUri = resolvedAuthorProfileImageUri,
        };

        return (ServiceResultCode.Success, threadDto);
    }

    public async Task<(ServiceResultCode, IReadOnlyCollection<HiddenThreadDto>)> GetHiddenThreadsAsync(
        string authorUserName,
        string requestUserName)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (!await _userPermissionValidator.IsUserAllowedToReviewHiddenPostAsync(requestUserName))
        {
            return (ServiceResultCode.Unauthorized, []);
        }

        var hiddenThreads = await dbContext.Thread
            .AsNoTracking()
            .Include(b => b.AuthorUser)
            .Where(b => b.AuthorUser.UserName == authorUserName && b.IsHidden)
            .Select(b => new HiddenThreadDto
            {
                Id = b.Id,
                Title = b.Title,
                Introduction = b.Introduction,
                Content = b.Body,
                CreationTime = b.CreationTime,
            })
            .ToListAsync();

        return (ServiceResultCode.Success, hiddenThreads);
    }
}
