using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RazorBlog.Core.Data;
using RazorBlog.Core.Models;
using RazorBlog.Core.Services;
using RazorBlog.IntegrationTest.Factories;
using RazorBlog.Web.Pages.Blogs;
using System.Net;
using System.Security.Claims;
using Xunit;

namespace RazorBlog.IntegrationTest.Pages;

public class BlogReadPageTest : BaseTest
{
    public BlogReadPageTest(RazorBlogApplicationFactory webApplicationFactory) : base(webApplicationFactory)
    {
    }

    [Fact]
    private async Task GetBlog_ShouldReturnNotFound_IfBlogIsNotFound()
    {
        var httpClient = ApplicationFactory.CreateClient();

        await using var scope = CreateScope();
        await using var dbContext = CreateDbContext(scope);
        var existingBlogIds = dbContext.Blog.Select(x => x.Id).ToHashSet();
        var faker = new Faker();
        var blogId = faker.Random.Int(); 
        while (existingBlogIds.Contains(blogId))
        {
            blogId = faker.Random.Int();
        }

        var url = $"/blogs/Read?id={blogId}";
        var response = await httpClient.GetAsync(url);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.RequestMessage!.RequestUri!.PathAndQuery.Should().BeEquivalentTo("/Error/Error/?ErrorMessage=Not%20Found&ErrorDescription=Resource%20not%20found");
    }

    [Fact]
    private async Task GetBlog_ShouldReturnBlogPage_IfBlogIsFound()
    {
        var faker = new Faker();

        await using var scope = CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await using var dbContext = CreateDbContext(scope);
        var user = new ApplicationUser
        {
            UserName = faker.Name.LastName(),
            EmailConfirmed = true,
            RegistrationDate = DateTime.Now,
            ProfileImageUri = faker.Internet.Url(),
        };

        var userResult = await userManager.CreateAsync(user, "TestPassword999@@");
        userResult.Succeeded.Should().BeTrue();
        var blog = new Blog
        {
            AuthorUserName = user.UserName,
            Body = faker.Lorem.Paragraph(),
            Title = faker.Lorem.Sentence(),
            Introduction = faker.Lorem.Sentences(2),
        };

        dbContext.Blog.Add(blog);
        await dbContext.SaveChangesAsync();

        var existingBlogIds = dbContext.Blog.Select(x => x.Id).ToHashSet();

        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var pageModel = new ReadModel(
            scope.ServiceProvider.GetRequiredService<RazorBlogDbContext>(),
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
            scope.ServiceProvider.GetRequiredService<ILogger<ReadModel>>(),
            scope.ServiceProvider.GetRequiredService<IPostModerationService>(),
            scope.ServiceProvider.GetRequiredService<IBlogContentManager>(),
            scope.ServiceProvider.GetRequiredService<IUserPermissionValidator>(),
            scope.ServiceProvider.GetRequiredService<IBlogReader>())
        {
            PageContext = new PageContext(actionContext) { ViewData = new ViewDataDictionary(modelMetadataProvider, modelState) },
            Url = new UrlHelper(actionContext)
        };

        pageModel.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(authenticationType: null));

        var pageModelResult = await pageModel.OnGetAsync(blog.Id);

        pageModelResult.Should().BeOfType<PageResult>();
        pageModel.DetailedBlogDto.Title.Should().BeEquivalentTo(blog.Title);
        pageModel.DetailedBlogDto.Introduction.Should().BeEquivalentTo(blog.Introduction);
        pageModel.DetailedBlogDto.Content.Should().BeEquivalentTo(blog.Body);
        pageModel.DetailedBlogDto.AuthorName.Should().BeEquivalentTo(blog.AuthorUserName);
    }
}