using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SimpleForum.Core.Data;

namespace SimpleForum.UnitTests.Utils;

internal class DatabaseTestUtil
{
    internal static async Task<SimpleForumDbContext> CreateInMemorySqliteDbMock()
    {
        var newConnection = new SqliteConnection("DataSource=:memory:;");
        await newConnection.OpenAsync();

        var dbContext = new SimpleForumDbContext(
            new DbContextOptionsBuilder<SimpleForumDbContext>()
                .UseSqlite(newConnection)
                .Options);

        await dbContext.Database.EnsureCreatedAsync();

        return dbContext;
    }

    internal static async Task<SimpleForumDbContext> CreateDbDummy()
    {
        var newConnection = new SqliteConnection("DataSource=:memory:;");
        await newConnection.OpenAsync();

        var dbContext = new SimpleForumDbContext(
             new DbContextOptionsBuilder<SimpleForumDbContext>()
                .UseSqlite(newConnection)
                .Options);

        return dbContext;
    }
}
