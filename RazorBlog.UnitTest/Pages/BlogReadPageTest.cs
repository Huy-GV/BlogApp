using System.Security.Claims;
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
using Microsoft.Extensions.Logging;
using Moq;
using RazorBlog.Core.Communication;
using RazorBlog.Core.Data;
using RazorBlog.Core.Models;
using RazorBlog.Core.Services;
using RazorBlog.UnitTest.Utils;
using RazorBlog.Web.Pages.Blogs;

namespace RazorBlog.UnitTest.Pages;

public class BlogReadPageTest
{
    private readonly Mock<IBlogContentManager> _mockBlogContentManager = new();
    private readonly Mock<ILogger<ReadModel>> _mockLogger = new();
    private readonly Mock<IUserPermissionValidator> _mockUserPermissionValidator = new();
    private readonly Mock<IBlogReader> _mockBlogReader = new();
    private readonly Mock<IPostModerationService> _mockPostModerationService = new();

    private ReadModel CreateTestSubject(
        RazorBlogDbContext mockAppDbContext,
        UserManager<ApplicationUser> mockUserManager,
        TempDataDictionary tempData,
        PageContext pageContext,
        UrlHelper urlHelper)
    {
        return new ReadModel(
            mockAppDbContext,
            mockUserManager,
            _mockLogger.Object,
            _mockPostModerationService.Object,
            _mockBlogContentManager.Object,
            _mockUserPermissionValidator.Object,
            _mockBlogReader.Object)
        {
            PageContext = pageContext,
            TempData = tempData,
            Url = urlHelper
        };
    }

    [Fact]
    private async Task OnGetAsync_ShouldReturnNotFound_IfBlogIsNotFound()
    {
        await using var mockAppDbContext = await DatabaseTestUtil.CreateMockSqliteDatabase();

        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var pageModel = CreateTestSubject(
            mockAppDbContext,
            UserManagerTestUtil.CreateMockUserManager().Object,
            new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>()),
            new PageContext(actionContext) { ViewData = new ViewDataDictionary(modelMetadataProvider, modelState) },
            new UrlHelper(actionContext));

        var faker = new Faker();
        var blogId = faker.Random.Int(0, int.MaxValue);
        _mockBlogReader
            .Setup(x => x.GetBlogAsync(blogId))
            .ReturnsAsync((ServiceResultCode.NotFound, null));

        var result = await pageModel.OnGetAsync(blogId);
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    private async Task OnPostHideBlogAsync_ShouldReturnChallenge_IfUserIsUnauthenticated()
    {
        await using var mockAppDbContext = await DatabaseTestUtil.CreateMockSqliteDatabase();

        var principal = new ClaimsPrincipal(new ClaimsIdentity(authenticationType: null));
        var httpContext = new Mock<HttpContext>();
        httpContext.Setup(x => x.User).Returns(principal);

        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(httpContext.Object, new RouteData(), new PageActionDescriptor(), modelState);
        var modelMetadataProvider = new EmptyModelMetadataProvider();

        var pageModel = CreateTestSubject(
            mockAppDbContext,
            UserManagerTestUtil.CreateMockUserManager().Object,
            new TempDataDictionary(httpContext.Object, Mock.Of<ITempDataProvider>()),
            new PageContext(actionContext) { ViewData = new ViewDataDictionary(modelMetadataProvider, modelState) },
            new UrlHelper(actionContext));

        var faker = new Faker();
        var blogId = faker.Random.Int(0, int.MaxValue);

        var result = await pageModel.OnPostHideBlogAsync(blogId);
        result.Should().BeOfType<ChallengeResult>();
    }
}
