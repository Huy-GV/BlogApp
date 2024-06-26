using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RazorBlog.Core.Data;
using RazorBlog.IntegrationTest.Factories;
using Xunit;

namespace RazorBlog.IntegrationTest.Pages;
public abstract class BaseTest : IClassFixture<RazorBlogApplicationFactory>
{
    public BaseTest(RazorBlogApplicationFactory webApplicationFactory)
    {
        ApplicationFactory = webApplicationFactory;
    }

    protected RazorBlogApplicationFactory ApplicationFactory { get; }

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