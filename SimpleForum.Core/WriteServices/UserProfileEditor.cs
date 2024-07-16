using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SimpleForum.Core.Communication;
using SimpleForum.Core.Data;
using SimpleForum.Core.Data.ViewModels;

namespace SimpleForum.Core.WriteServices;

internal class UserProfileEditor : IUserProfileEditor
{
    private readonly IDbContextFactory<SimpleForumDbContext> _dbContextFactory;
    private readonly IImageStore _imageStore;
    private readonly ILogger<UserProfileEditor> _logger;
    public UserProfileEditor(
        IDbContextFactory<SimpleForumDbContext> dbContextFactory,
        IImageStore imageStore,
        ILogger<UserProfileEditor> logger)
    {
        _dbContextFactory = dbContextFactory;
        _imageStore = imageStore;
        _logger = logger;
    }

    public async Task<ServiceResultCode> EditProfileSummary(string userName, EditProfileSummaryViewModel viewModel)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var applicationUser = await dbContext.ApplicationUser.FirstOrDefaultAsync(x => x.UserName == userName);
        if (applicationUser == null)
        {
            return ServiceResultCode.Unauthorized;
        }

        applicationUser.Description = viewModel.Summary;
        await dbContext.SaveChangesAsync();

        return ServiceResultCode.Success;
    }

    public async Task<ServiceResultCode> ChangeProfilePicture(string userName, EditProfilePictureViewModel viewModel)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var applicationUser = await dbContext.ApplicationUser.FirstOrDefaultAsync(x => x.UserName == userName);
        if (applicationUser == null)
        {
            return ServiceResultCode.Unauthorized;
        }

        var (result, imageUri) = await _imageStore.UploadProfileImageAsync(viewModel.NewProfilePicture);
        if (result == ServiceResultCode.Success)
        {
            _logger.LogInformation("Deleting previous profile image of user named '{userName}')", userName);
            await _imageStore.DeleteImage(applicationUser.ProfileImageUri);
            applicationUser.ProfileImageUri = imageUri!;
        }
        else
        {
            return result;
        }

        await dbContext.SaveChangesAsync();
        return ServiceResultCode.Success;
    }

    public async Task<ServiceResultCode> RemoveProfilePicture(string userName)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var applicationUser = await dbContext.ApplicationUser.FirstOrDefaultAsync(x => x.UserName == userName);
        if (applicationUser == null)
        {
            return ServiceResultCode.Unauthorized;
        }

        applicationUser.ProfileImageUri = string.Empty;
        await dbContext.SaveChangesAsync();
        return ServiceResultCode.Success;
    }
}
