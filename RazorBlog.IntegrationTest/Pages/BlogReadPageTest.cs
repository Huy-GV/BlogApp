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
using RazorBlog.Core.Communication;
using RazorBlog.Core.Data.Dtos;
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
        await using var dbContext = CreateDbContext(scope.ServiceProvider);
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

    [Theory]
    [InlineData("Author1", "", "", "", false, false)]
    [InlineData("Author2", "", "User1", "", true, false)]
    [InlineData("Author3", "", "User2", "", true, true)]
    [InlineData("Author4", "", "Author4", "", true, false)]
    [InlineData("Author5", "", "moderator", "moderator", true, false)]
    [InlineData("Author6", "", "admin", "admin", true, false)]
    [InlineData("admin", "", "moderator", "moderator", true, false)]
    private async Task GetBlog_ShouldReturnBlogPage_IfBlogIsFound(
        string authorUserName,
        string authorUserRole,
        string visitorUserName,
        string visitorUserRole,
        bool isVisitorUserAuthenticated,
        bool isVisitorUserBanned)
    {
        async Task<ApplicationUser> EnsureUserExists(IServiceProvider serviceProvider, string userName, string role = "")
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var faker = new Faker();
            var existing = await userManager.FindByNameAsync(userName);
            if (existing != null)
            {
                return existing;
            }

            var user = new ApplicationUser
            {
                UserName = userName,
                EmailConfirmed = true,
                RegistrationDate = DateTime.UtcNow,
                ProfileImageUri = faker.Internet.Url()
            };

            var ensureUserExistsResult = await userManager.CreateAsync(user, "TestPassword999@@");
            ensureUserExistsResult.Succeeded
                .Should()
                .BeTrue(string.Join("\n", ensureUserExistsResult.Errors.Select(x => x.Description)));

            if (!string.IsNullOrEmpty(role))
            {
                var assignRoleResult = await userManager.AddToRoleAsync(user, role);
                assignRoleResult.Succeeded
                    .Should()
                    .BeTrue(string.Join("\n", assignRoleResult.Errors.Select(x => x.Description)));
            }

            return user;
        }

        async Task SetUpVisitorUserBanned(IServiceProvider serviceProvider, bool isBanned)
        {
            if (!isVisitorUserBanned)
            {
                return;
            }

            var faker = new Faker();
            var userModerationService = serviceProvider.GetRequiredService<IUserModerationService>();
            var banUserResult = await userModerationService.BanUserAsync(visitorUserName, "admin", faker.Date.Future());
            banUserResult.Should().Be(ServiceResultCode.Success);
        }

        async Task<Blog> SetUpBlogCreated(IServiceProvider serviceProvider)
        {
            var faker = new Faker();
            await using var dbContext = CreateDbContext(serviceProvider);
            var blog = new Blog
            {
                AuthorUserName = authorUserName,
                Body = faker.Lorem.Paragraph(),
                Title = faker.Lorem.Sentence(),
                Introduction = faker.Lorem.Sentences(2),
            };

            dbContext.Blog.Add(blog);
            await dbContext.SaveChangesAsync();

            return blog;
        }

        var faker = new Faker();
        await using var scope = CreateScope();

        await EnsureUserExists(scope.ServiceProvider, authorUserName, authorUserRole);
        var blog = await SetUpBlogCreated(scope.ServiceProvider);

        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var pageModel = ActivatorUtilities.CreateInstance<ReadModel>(scope.ServiceProvider);

        pageModel.PageContext = new PageContext(actionContext) { ViewData = new ViewDataDictionary(modelMetadataProvider, modelState) };
        pageModel.Url = new UrlHelper(actionContext);

        if (isVisitorUserAuthenticated)
        {
            var visitorUser = await EnsureUserExists(scope.ServiceProvider, visitorUserName, visitorUserRole);
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, visitorUserName),
                new(ClaimTypes.Role, visitorUserRole),
                new(ClaimTypes.NameIdentifier, visitorUser!.Id)
            };

            pageModel.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Test"));
            await SetUpVisitorUserBanned(scope.ServiceProvider, isVisitorUserBanned);
        } else
        {
            pageModel.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(authenticationType: null));
        }

        var pageModelResult = await pageModel.OnGetAsync(blog.Id);

        pageModelResult.Should().BeOfType<PageResult>();
        pageModel.DetailedBlogDto.Title.Should().BeEquivalentTo(blog.Title);
        pageModel.DetailedBlogDto.Introduction.Should().BeEquivalentTo(blog.Introduction);
        pageModel.DetailedBlogDto.Content.Should().BeEquivalentTo(blog.Body);
        pageModel.DetailedBlogDto.AuthorName.Should().BeEquivalentTo(blog.AuthorUserName);

        var isVisitorUserAuthor = visitorUserName == authorUserName;
        var isAdminOrModerator = visitorUserRole is "admin" or "moderator";
        var expectedUserInfo = new CurrentUserInfo
        {
            UserName = visitorUserName,
            AllowedToHidePost = isVisitorUserAuthenticated && isAdminOrModerator && authorUserRole != "admin",
            AllowedToModifyOrDeletePost = isVisitorUserAuthenticated && isVisitorUserAuthor,
            AllowedToCreateComment = isVisitorUserAuthenticated && !isVisitorUserBanned,
        };

        pageModel.CurrentUserInfo.Should().BeEquivalentTo(expectedUserInfo);
    }
}
