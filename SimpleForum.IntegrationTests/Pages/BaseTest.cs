using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleForum.Core.Data;
using SimpleForum.IntegrationTests.Fixtures;
using Xunit;

namespace SimpleForum.IntegrationTests.Pages;
public abstract class BaseTest : IClassFixture<TestWebAppFactoryFixture>
{
    public BaseTest(TestWebAppFactoryFixture webApplicationFactory)
    {
        ApplicationFactory = webApplicationFactory;
    }

    protected TestWebAppFactoryFixture ApplicationFactory { get; }

    internal AsyncServiceScope CreateScope()
    {
        return ApplicationFactory.Services.CreateAsyncScope();
    }

    internal static SimpleForumDbContext CreateDbContext(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<DbContextOptions<SimpleForumDbContext>>();

        return new SimpleForumDbContext(options);
    }
}
