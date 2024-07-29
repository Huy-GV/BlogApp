using System;
using System.Data;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SimpleForum.Core.Communication;
using SimpleForum.Core.Data;
using SimpleForum.Core.Data.Constants;
using SimpleForum.Core.Models;

namespace SimpleForum.Core.CommandServices;

internal class UserModerationService : IUserModerationService
{
    private readonly SimpleForumDbContext _dbContext;
    private readonly ILogger<UserModerationService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public UserModerationService(SimpleForumDbContext dbContext,
        ILogger<UserModerationService> logger,
        UserManager<ApplicationUser> userManager,
        IBackgroundJobClient backgroundJobClient)
    {
        _dbContext = dbContext;
        _logger = logger;
        _userManager = userManager;
        _backgroundJobClient = backgroundJobClient;
    }

    private async Task<bool> IsUserNameFromAdminUser(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        return user != null && await _userManager.IsInRoleAsync(user, Roles.AdminRole);
    }

    public async Task PrivateRemoveBanTicketAsync(string bannedUserName)
    {
        var banTicket = await _dbContext.BanTicket.FirstOrDefaultAsync(x => x.UserName == bannedUserName);
        if (banTicket == null)
        {
            _logger.LogWarning("Ban ticket for user {bannedUserName} already removed", bannedUserName);
            return;
        }

        _dbContext.BanTicket.Remove(banTicket);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DBConcurrencyException)
        {
            _logger.LogWarning("Ban ticket for user {bannedUserName} already removed", bannedUserName);
        }
    }

    public async Task<ServiceResultCode> BanUserAsync(string userToBanName, string userName, DateTime? expiry)
    {
        if (!await IsUserNameFromAdminUser(userName))
        {
            return ServiceResultCode.Unauthorized;
        }

        var now = DateTime.UtcNow.Date;
        if (expiry.HasValue && expiry.Value.Date <= now)
        {
            return ServiceResultCode.InvalidArguments;
        }

        if (await _dbContext.BanTicket.AnyAsync(x => x.UserName == userToBanName))
        {
            _logger.LogInformation("User named {userToBanName} has already been banned", userToBanName);
            return ServiceResultCode.InvalidArguments;
        }

        var user = await _userManager.FindByNameAsync(userToBanName);
        if (user == null)
        {
            _logger.LogError("User named {userToBanName} not found", userToBanName);
            return ServiceResultCode.NotFound;
        }

        await _userManager.RemoveFromRoleAsync(user, Roles.ModeratorRole);
        _dbContext.BanTicket.Add(new BanTicket { UserName = userToBanName, Expiry = expiry });
        await _dbContext.SaveChangesAsync();

        if (expiry.HasValue)
        {
            _backgroundJobClient.Schedule(() => PrivateRemoveBanTicketAsync(userToBanName), new DateTimeOffset(expiry.Value));
        }

        return ServiceResultCode.Success;
    }

    public async Task<ServiceResultCode> RemoveBanTicketAsync(string bannedUserName, string userName)
    {
        if (!await IsUserNameFromAdminUser(userName))
        {
            return ServiceResultCode.Unauthorized;
        }

        await PrivateRemoveBanTicketAsync(bannedUserName);
        return ServiceResultCode.Success;
    }
}
