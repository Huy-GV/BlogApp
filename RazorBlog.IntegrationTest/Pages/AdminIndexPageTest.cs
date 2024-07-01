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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RazorBlog.Core.Data;
using RazorBlog.Core.Models;
using RazorBlog.IntegrationTest.Fixtures;
using System.Security.Claims;
using Xunit;
using IndexModel = RazorBlog.Web.Pages.Admin.IndexModel;

namespace RazorBlog.IntegrationTest.Pages;
public class AdminIndexPageTest : BaseTest
{
    public AdminIndexPageTest(TestWebAppFactoryFixture webApplicationFactory) : base(webApplicationFactory)
    {
    }

    private async Task<ClaimsPrincipal> SetUpAdminClaimsPrincipal(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await userManager.FindByNameAsync("admin");
        admin.Should().NotBeNull();
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "admin"),
            new(ClaimTypes.Role, "admin"),
            new(ClaimTypes.NameIdentifier, admin!.Id)
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Test"));
    }

    [Fact]
    private async Task OnPostAssignModeratorRoleAsync_ShouldAddUserToModeratorList()
    {
        var faker = new Faker();
        await using var scope = CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<RazorBlogDbContext>();

        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var pageModel = ActivatorUtilities.CreateInstance<IndexModel>(scope.ServiceProvider);

        pageModel.PageContext = new PageContext(actionContext) { ViewData = new ViewDataDictionary(modelMetadataProvider, modelState) };
        pageModel.Url = new UrlHelper(actionContext);

        var adminAndModeratorUserIds = await dbContext.UserRoles
            .Select(x => x.UserId)
            .Distinct()
            .ToListAsync();

        var normalUsers = await dbContext.Users
            .Where(x => !adminAndModeratorUserIds.Contains(x.Id))
            .ToListAsync();

        var randomUserIndex = faker.Random.Int(min: 0, max: normalUsers.Count - 1);
        var userToAssignModeratorRole = normalUsers[randomUserIndex];

        pageModel.HttpContext.User = await SetUpAdminClaimsPrincipal(scope.ServiceProvider);

        var getResult = await pageModel.OnGetAsync();
        getResult.Should().BeOfType<PageResult>();
        pageModel.NormalUsers.Select(x => x.UserName).Should().Contain(userToAssignModeratorRole.UserName);
        pageModel.Moderators.Select(x => x.UserName).Should().NotContain(userToAssignModeratorRole.UserName);

        var postResult = await pageModel.OnPostAssignModeratorRoleAsync(userToAssignModeratorRole.UserName!);
        postResult.Should().BeOfType<RedirectToPageResult>();

        pageModel.NormalUsers.Clear();
        pageModel.Moderators.Clear();

        await pageModel.OnGetAsync();
        pageModel.Moderators.Select(x => x.UserName).Should().Contain(userToAssignModeratorRole.UserName);
        pageModel.NormalUsers.Select(x => x.UserName).Should().NotContain(userToAssignModeratorRole.UserName);
    }

    [Fact]
    private async Task OnPostRemoveModeratorRoleAsync_ShouldRemoveUserFromModeratorList()
    {
        var faker = new Faker();
        await using var scope = CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var pageModel = ActivatorUtilities.CreateInstance<IndexModel>(scope.ServiceProvider);

        pageModel.PageContext = new PageContext(actionContext) { ViewData = new ViewDataDictionary(modelMetadataProvider, modelState) };
        pageModel.Url = new UrlHelper(actionContext);

        var moderatorUsers = await userManager.GetUsersInRoleAsync("moderator");
        var randomUserIndex = faker.Random.Int(min: 0, max: moderatorUsers.Count - 1);
        var userToUnassignModeratorRole = moderatorUsers[randomUserIndex];

        pageModel.HttpContext.User = await SetUpAdminClaimsPrincipal(scope.ServiceProvider);

        var getResult = await pageModel.OnGetAsync();
        getResult.Should().BeOfType<PageResult>();
        pageModel.NormalUsers.Select(x => x.UserName).Should().NotContain(userToUnassignModeratorRole.UserName);
        pageModel.Moderators.Select(x => x.UserName).Should().Contain(userToUnassignModeratorRole.UserName);

        var postResult = await pageModel.OnPostRemoveModeratorRoleAsync(userToUnassignModeratorRole.UserName!);
        postResult.Should().BeOfType<RedirectToPageResult>();

        pageModel.NormalUsers.Clear();
        pageModel.Moderators.Clear();

        await pageModel.OnGetAsync();
        pageModel.Moderators.Select(x => x.UserName).Should().NotContain(userToUnassignModeratorRole.UserName);
        pageModel.NormalUsers.Select(x => x.UserName).Should().Contain(userToUnassignModeratorRole.UserName);
    }
}
