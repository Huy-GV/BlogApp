using Microsoft.EntityFrameworkCore;
using SimpleForum.Core.Data;
using SimpleForum.Core.Models;
using System.Threading.Tasks;

namespace SimpleForum.Core.QueryServices;
internal class BanTicketReader : IBanTicketReader
{
    private readonly IDbContextFactory<SimpleForumDbContext> _dbContextFactory;
    public BanTicketReader(
        IDbContextFactory<SimpleForumDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<bool> BanTicketExistsAsync(string userName)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.BanTicket.AnyAsync(s => s.UserName == userName);
    }

    public async Task<BanTicket?> FindBanTicketByUserNameAsync(string userName)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext
            .BanTicket
            .Include(x => x.AppUser)
            .FirstOrDefaultAsync(s => s.UserName == userName);
    }
}
