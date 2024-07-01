using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RazorBlog.Core.Data;
using RazorBlog.IntegrationTest.Fixtures;
using Xunit;

namespace RazorBlog.IntegrationTest.Pages;
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

    internal static RazorBlogDbContext CreateDbContext(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<DbContextOptions<RazorBlogDbContext>>();

        return new RazorBlogDbContext(options);
    }
}