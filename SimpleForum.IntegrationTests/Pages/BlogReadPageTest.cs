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
using SimpleForum.Core.Communication;
using SimpleForum.Core.Data.Dtos;
using SimpleForum.Core.Models;
using SimpleForum.Core.WriteServices;
using SimpleForum.IntegrationTests.Fixtures;
using SimpleForum.Web.Pages.Threads;
using System.Net;
using System.Security.Claims;
using Xunit;

namespace SimpleForum.IntegrationTests.Pages;

public class ThreadReadPageTest : BaseTest
{
    public ThreadReadPageTest(TestWebAppFactoryFixture webApplicationFactory) : base(webApplicationFactory)
    {
    }

    [Fact]
    private async Task GetThread_ShouldReturnNotFound_IfThreadIsNotFound()
    {
        var httpClient = ApplicationFactory.CreateClient();
        await using var scope = CreateScope();
        await using var dbContext = CreateDbContext(scope.ServiceProvider);
        var existingThreadIds = dbContext.Thread.Select(x => x.Id).ToHashSet();
        var faker = new Faker();
        var threadId = Math.Abs(faker.Random.Int());
        while (existingThreadIds.Contains(threadId))
        {
            threadId = Math.Abs(faker.Random.Int());
        }

        var url = $"/threads/Read?id={threadId}";
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
    [InlineData("admin", "admin", "moderator", "moderator", true, false)]
    private async Task GetThread_ShouldReturnThreadPage_IfThreadIsFound(
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

        async Task<Core.Models.Thread> SetUpThreadCreated(IServiceProvider serviceProvider)
        {
            var faker = new Faker();
            await using var dbContext = CreateDbContext(serviceProvider);
            var thread = new Core.Models.Thread
            {
                AuthorUserName = authorUserName,
                Body = faker.Lorem.Paragraph(),
                Title = faker.Lorem.Sentence(),
                Introduction = faker.Lorem.Sentences(2),
            };

            dbContext.Thread.Add(thread);
            await dbContext.SaveChangesAsync();

            return thread;
        }

        var faker = new Faker();
        await using var scope = CreateScope();

        await EnsureUserExists(scope.ServiceProvider, authorUserName, authorUserRole);
        var thread = await SetUpThreadCreated(scope.ServiceProvider);

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

        var pageModelResult = await pageModel.OnGetAsync(thread.Id);

        pageModelResult.Should().BeOfType<PageResult>();
        pageModel.DetailedThreadDto.Title.Should().BeEquivalentTo(thread.Title);
        pageModel.DetailedThreadDto.Introduction.Should().BeEquivalentTo(thread.Introduction);
        pageModel.DetailedThreadDto.Content.Should().BeEquivalentTo(thread.Body);
        pageModel.DetailedThreadDto.AuthorName.Should().BeEquivalentTo(thread.AuthorUserName);

        var isVisitorUserAuthor = visitorUserName == authorUserName;
        var isAdminOrModerator = visitorUserRole is "admin" or "moderator";
        var expectedUserInfo = new UserPermissionsDto
        {
            UserName = visitorUserName,
            AllowedToReportPost = isVisitorUserAuthenticated && isAdminOrModerator && authorUserRole != "admin",
            AllowedToModifyOrDeletePost = isVisitorUserAuthenticated && isVisitorUserAuthor,
            AllowedToCreateComment = isVisitorUserAuthenticated && !isVisitorUserBanned,
        };

        pageModel.UserPermissionsDto.Should().BeEquivalentTo(expectedUserInfo);
    }
}
