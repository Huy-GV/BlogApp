using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using SimpleForum.Core.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SimpleForumDbContext>
{
    public DesignTimeDbContextFactory()
    {
        // required by the EF Core CLI tools.
    }

    public SimpleForumDbContext CreateDbContext(string[] args)
    {
        // EF Core migration tools cannot work properly in 2024
        var connectionString = Environment.GetEnvironmentVariable("ConnectionString");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("ConnectionString must not be null");
        }

        var options = new DbContextOptionsBuilder<SimpleForumDbContext>()
           .UseSqlServer(connectionString)
           .Options;

        return new SimpleForumDbContext(options);
    }
}