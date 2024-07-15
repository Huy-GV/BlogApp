using Microsoft.EntityFrameworkCore;
using RazorBlog.Core.Data;
using RazorBlog.Core.Models;
using System.Threading.Tasks;

namespace RazorBlog.Core.ReadServices;
internal class BanTicketReader : IBanTicketReader
{
    private readonly IDbContextFactory<RazorBlogDbContext> _dbContextFactory;
    public BanTicketReader(
        IDbContextFactory<RazorBlogDbContext> dbContextFactory)
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
