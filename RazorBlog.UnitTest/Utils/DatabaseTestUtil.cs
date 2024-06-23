using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RazorBlog.Core.Data;

namespace RazorBlog.UnitTest.Utils;

internal class DatabaseTestUtil
{
    internal static async Task<RazorBlogDbContext> CreateMockSqliteDatabase()
    {
        var newConnection = new SqliteConnection("DataSource=:memory:;");
        await newConnection.OpenAsync();

        var dbContext = new RazorBlogDbContext(
            new DbContextOptionsBuilder<RazorBlogDbContext>()
                .UseSqlite(newConnection)
                .Options);

        await dbContext.Database.EnsureCreatedAsync();

        return dbContext;
    }
}