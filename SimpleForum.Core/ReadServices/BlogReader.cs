using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimpleForum.Core.Communication;
using SimpleForum.Core.Data;
using SimpleForum.Core.Data.Constants;
using SimpleForum.Core.Data.Dtos;

namespace SimpleForum.Core.ReadServices;

internal class BlogReader : IBlogReader
{
    private readonly IDbContextFactory<SimpleForumDbContext> _dbContextFactory;
    private readonly IAggregateImageUriResolver _aggregateImageUriResolver;
    private readonly IUserPermissionValidator _userPermissionValidator;
    public BlogReader(
        IDbContextFactory<SimpleForumDbContext> dbContextFactory,
        IAggregateImageUriResolver aggregateImageUriResolver,
        IUserPermissionValidator userPermissionValidator)
    {
        _dbContextFactory = dbContextFactory;
        _aggregateImageUriResolver = aggregateImageUriResolver;
        _userPermissionValidator = userPermissionValidator;
    }

    public async Task<IReadOnlyCollection<IndexBlogDto>> GetBlogsAsync(
        string? searchString = null,
        int page = 0,
        int pageSize = 10)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var blogs = await dbContext.Blog
            .Include(b => b.AuthorUser)
            .Include(b => b.Comments)
            .ThenInclude(c => c.AuthorUser)
            .Where(x => !x.IsHidden)
            .Select(b => new IndexBlogDto
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

        return await Task.WhenAll(blogs
            .Select(async x =>
            {
                x.CoverImageUri = await _aggregateImageUriResolver.ResolveImageUriAsync(x.CoverImageUri) ?? string.Empty;
                return x;
            }).ToList());
    }

    public async Task<(ServiceResultCode, DetailedBlogDto?)> GetBlogAsync(int id)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var blog = await dbContext.Blog
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(blog => blog.AuthorUser)
            .FirstOrDefaultAsync(blog => blog.Id == id);

        if (blog == null)
        {
            return (ServiceResultCode.NotFound, null);
        }

        //blog.ViewCount++;
        //await _dbContext.SaveChangesAsync();

        var resolvedCoverImageUri = await _aggregateImageUriResolver.ResolveImageUriAsync(blog.CoverImageUri)
            ?? string.Empty;
        var resolvedAuthorProfileImageUri = await _aggregateImageUriResolver.ResolveImageUriAsync(blog.AuthorUser.ProfileImageUri)
            ?? string.Empty;

        var blogDto = new DetailedBlogDto
        {
            Id = blog.Id,
            Introduction = blog.IsHidden ? ReplacementText.HiddenContent : blog.Introduction,
            Title = blog.IsHidden ? ReplacementText.HiddenContent : blog.Title,
            Content = blog.IsHidden ? ReplacementText.HiddenContent : blog.Body,
            CoverImageUri = resolvedCoverImageUri,
            CreationTime = blog.CreationTime,
            LastUpdateTime = blog.LastUpdateTime,
            IsHidden = blog.IsHidden,
            AuthorDescription = blog.AuthorUser.Description,
            AuthorName = blog.AuthorUser.UserName ?? ReplacementText.DeletedUser,
            AuthorProfileImageUri = resolvedAuthorProfileImageUri,
        };

        return (ServiceResultCode.Success, blogDto);
    }

    public async Task<(ServiceResultCode, IReadOnlyCollection<HiddenBlogDto>)> GetHiddenBlogsAsync(
        string authorUserName,
        string requestUserName)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (!await _userPermissionValidator.IsUserAllowedToReviewHiddenPostAsync(requestUserName))
        {
            return (ServiceResultCode.Unauthorized, []);
        }

        var hiddenBlogs = await dbContext.Blog
            .AsNoTracking()
            .Include(b => b.AuthorUser)
            .Where(b => b.AuthorUser.UserName == authorUserName && b.IsHidden)
            .Select(b => new HiddenBlogDto
            {
                Id = b.Id,
                Title = b.Title,
                Introduction = b.Introduction,
                Content = b.Body,
                CreationTime = b.CreationTime,
            })
            .ToListAsync();

        return (ServiceResultCode.Success, hiddenBlogs);
    }
}
