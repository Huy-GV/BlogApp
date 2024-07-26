using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimpleForum.Core.Communication;
using SimpleForum.Core.Data;
using SimpleForum.Core.Data.Dtos;

namespace SimpleForum.Core.ReadServices;

internal class UserProfileReader : IUserProfileReader
{
    private readonly IDbContextFactory<SimpleForumDbContext> _dbContextFactory;
    private readonly IAggregateImageUriResolver _aggregateImageUriResolver;
    private readonly IDefaultProfileImageProvider _defaultProfileImageProvider;
    public UserProfileReader(
        IDbContextFactory<SimpleForumDbContext> dbContextFactory,
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

        var threads = dbContext.Thread
            .Include(thread => thread.AuthorUser)
            .AsNoTracking()
            .Where(thread => thread.AuthorUser.UserName == userName)
            .ToList();

        var threadsGroupedByYear = threads
            .GroupBy(b => b.CreationTime.Year)
            .OrderByDescending(g => g.Key)
            .ToDictionary(
                group => (uint)group.Key,
                group => group.Select(b => new MinimalThreadDto
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
            ThreadCount = (uint)threads.Count,
            ProfileImageUri = await _aggregateImageUriResolver.ResolveImageUriAsync(user.ProfileImageUri)
                              ?? await _defaultProfileImageProvider.GetDefaultProfileImageUriAsync(),
            ThreadsGroupedByYear = threadsGroupedByYear,
            Description = string.IsNullOrEmpty(user.Description)
                ? "None"
                : user.Description,
            CommentCount = (uint)dbContext.Comment
                .Include(c => c.AuthorUser)
                .Where(c => c.AuthorUser.UserName == userName)
                .ToList()
                .Count,
            ThreadCountCurrentYear = (uint)threads
                .Where(thread => thread.AuthorUser.UserName == userName &&
                               thread.CreationTime.Year == DateTime.Now.Year)
                .ToList()
                .Count,
            ViewCountCurrentYear = (uint)threads
                .Where(thread => thread.AuthorUser.UserName == userName &&
                               thread.CreationTime.Year == DateTime.Now.Year)
                .Sum(thread => thread.ViewCount),
            RegistrationDate = user.RegistrationDate == null
                    ? "a long time ago"
                    : user.RegistrationDate.Value.ToString("dd/MMMM/yyyy"),
        };

        return (ServiceResultCode.Success, profileDto);
    }
}
