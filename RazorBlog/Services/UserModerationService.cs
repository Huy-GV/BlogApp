using System;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RazorBlog.Communication;
using RazorBlog.Data;
using RazorBlog.Data.Constants;
using RazorBlog.Models;

namespace RazorBlog.Services;

public class UserModerationService(
    RazorBlogDbContext dbContext,
    ILogger<UserModerationService> logger,
    UserManager<ApplicationUser> userManager) : IUserModerationService
{
    private readonly RazorBlogDbContext _dbContext = dbContext;
    private readonly ILogger<UserModerationService> _logger = logger;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    private async Task<bool> IsUserNameFromAdminUser(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        return user != null && await _userManager.IsInRoleAsync(user, Roles.AdminRole);
    }

    public async Task RemoveBanTicketAsync(string bannedUserName)
    {
        var banTicket = await FindBanTicketByUserNameAsync(bannedUserName);
        if (banTicket == null)
        {
            _logger.LogWarning($"Ban ticket for user {bannedUserName} already removed");
            return;
        }

        try
        {
            _dbContext.BanTicket.Remove(banTicket);
            await _dbContext.SaveChangesAsync();
        }
        catch (DBConcurrencyException)
        {
            _logger.LogWarning($"Ban ticket for user {banTicket.UserName} already removed");
        }
    }

    public async Task<bool> BanTicketExistsAsync(string userName)
    {
        return await _dbContext.BanTicket.AnyAsync(s => s.UserName == userName);
    }

    public async Task<BanTicket?> FindBanTicketByUserNameAsync(string userName)
    {
        return await _dbContext
            .BanTicket
            .Include(x => x.AppUser)
            .FirstOrDefaultAsync(s => s.UserName == userName);
    }

    public async Task<ServiceResultCode> BanUserAsync(string userToBanName, string userName,  DateTime? expiry)
    {
        if (!await IsUserNameFromAdminUser(userName))
        {
            return ServiceResultCode.Unauthorized;
        }

        var now = DateTime.UtcNow;
        if (expiry.HasValue && expiry.Value <= now)
        {
            return ServiceResultCode.InvalidArguments;
        }

        if (await BanTicketExistsAsync(userToBanName))
        {
            _logger.LogInformation($"User {userToBanName} has already been banned");
        }

        var user = await _userManager.FindByNameAsync(userToBanName);
        if (user == null)
        {
            _logger.LogError($"User {userToBanName} not found");
            return ServiceResultCode.NotFound;
        }

        await _userManager.RemoveFromRoleAsync(user, Roles.ModeratorRole);
        _dbContext.BanTicket.Add(new BanTicket() { UserName = userToBanName, Expiry = expiry });
        await _dbContext.SaveChangesAsync();

        if (!expiry.HasValue)
        {
            return ServiceResultCode.Success;
        }

        BackgroundJob.Schedule(() => RemoveBanTicketAsync(userToBanName), new DateTimeOffset(expiry.Value));
        return ServiceResultCode.Success;
    }

    public async Task<ServiceResultCode> RemoveBanTicketAsync(string bannedUserName, string userName)
    {
        if (!await IsUserNameFromAdminUser(userName))
        {
            return ServiceResultCode.Unauthorized;
        }

        await RemoveBanTicketAsync(bannedUserName);
        return ServiceResultCode.Success;
    }
}