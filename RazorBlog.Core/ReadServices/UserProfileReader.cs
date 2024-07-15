using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RazorBlog.Core.Communication;
using RazorBlog.Core.Data;
using RazorBlog.Core.Data.Dtos;

namespace RazorBlog.Core.ReadServices;

internal class UserProfileReader : IUserProfileReader
{
    private readonly IDbContextFactory<RazorBlogDbContext> _dbContextFactory;
    private readonly IAggregateImageUriResolver _aggregateImageUriResolver;
    private readonly IDefaultProfileImageProvider _defaultProfileImageProvider;
    public UserProfileReader(
        IDbContextFactory<RazorBlogDbContext> dbContextFactory,
        IAggregateImageUriResolver aggregateImageUriResolver,
        IDefaultProfileImageProvider defaultProfileImageProvider)
    {
        _dbContextFactory = dbContextFactory;
        _aggregateImageUriResolver = aggregateImageUriResolver;
        _defaultProfileImageProvider = defaultProfileImageProvider;
    }

    public async Task<(ServiceResultCode, PersonalProfileDto?)> GetUserProfileAsync(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return (ServiceResultCode.InvalidArguments, null);
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserName == userName);
        if (user == null)
        {
            return (ServiceResultCode.InvalidArguments, null);
        }

        var blogs = dbContext.Blog
            .Include(b => b.AuthorUser)
            .AsNoTracking()
            .Where(blog => blog.AuthorUser.UserName == userName)
            .ToList();

        var blogsGroupedByYear = blogs
            .GroupBy(b => b.CreationTime.Year)
            .OrderByDescending(g => g.Key)
            .ToDictionary(
                group => (uint)group.Key,
                group => group.Select(b => new MinimalBlogDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    ViewCount = b.ViewCount,
                    CreationTime = b.CreationTime,
                })
            .ToList());

        var profileDto = new PersonalProfileDto
        {
            UserName = userName,
            BlogCount = (uint)blogs.Count,
            ProfileImageUri = await _aggregateImageUriResolver.ResolveImageUriAsync(user.ProfileImageUri)
                              ?? await _defaultProfileImageProvider.GetDefaultProfileImageUriAsync(),
            BlogsGroupedByYear = blogsGroupedByYear,
            Description = string.IsNullOrEmpty(user.Description)
                ? "None"
                : user.Description,
            CommentCount = (uint)dbContext.Comment
                .Include(c => c.AuthorUser)
                .Where(c => c.AuthorUser.UserName == userName)
                .ToList()
                .Count,
            BlogCountCurrentYear = (uint)blogs
                .Where(blog => blog.AuthorUser.UserName == userName &&
                               blog.CreationTime.Year == DateTime.Now.Year)
                .ToList()
                .Count,
            ViewCountCurrentYear = (uint)blogs
                .Where(blog => blog.AuthorUser.UserName == userName &&
                               blog.CreationTime.Year == DateTime.Now.Year)
                .Sum(blog => blog.ViewCount),
            RegistrationDate = user.RegistrationDate == null
                    ? "a long time ago"
                    : user.RegistrationDate.Value.ToString("dd/MMMM/yyyy"),
        };

        return (ServiceResultCode.Success, profileDto);
    }
}
