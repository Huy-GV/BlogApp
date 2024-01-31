using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RazorBlog.Communication;
using RazorBlog.Data;
using RazorBlog.Data.Constants;
using RazorBlog.Data.Dtos;

namespace RazorBlog.Services;

internal class BlogReader : IBlogReader
{
    private readonly RazorBlogDbContext _dbContext;
    private readonly IAggregateImageUriResolver _aggregateImageUriResolver;
    public BlogReader(RazorBlogDbContext dbContext, IAggregateImageUriResolver aggregateImageUriResolver)
    {
        _dbContext = dbContext;
        _aggregateImageUriResolver = aggregateImageUriResolver;
    }
    
    public async Task<IReadOnlyCollection<IndexBlogDto>> GetBlogsAsync(
        string? searchString = null, 
        int page = 0, 
        int pageSize = 10)
    {
        var blogs = await _dbContext.Blog
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
                x.CoverImageUri =  await _aggregateImageUriResolver.ResolveImageUriAsync(x.CoverImageUri) ?? string.Empty;
                return x;
            }).ToList());
    }
    
    public async Task<(ServiceResultCode, DetailedBlogDto?)> GetBlogAsync(int id)
    {
        var blog = await _dbContext.Blog
            .IgnoreQueryFilters()
            .Include(blog => blog.AuthorUser)
            .FirstOrDefaultAsync(blog => blog.Id == id);

        if (blog == null)
        {
            return (ServiceResultCode.NotFound, null);
        }
        
        _dbContext.Blog.Update(blog);
        blog.ViewCount++;
        await _dbContext.SaveChangesAsync();

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
}