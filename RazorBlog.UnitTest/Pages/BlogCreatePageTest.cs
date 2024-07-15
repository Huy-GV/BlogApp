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
using RazorBlog.Core.Data;
using RazorBlog.Core.Models;
using RazorBlog.Core.ReadServices;
using RazorBlog.Core.WriteServices;
using RazorBlog.UnitTest.Utils;
using RazorBlog.Web.Pages.Blogs;

namespace RazorBlog.UnitTest.Pages;

public class BlogCreatePageTest
{
    private readonly Mock<IBlogContentManager> _mockBlogContentManager = new();
    private readonly Mock<ILogger<CreateModel>> _mockLogger = new();
    private readonly Mock<IUserPermissionValidator> _mockUserPermissionValidator = new();

    private CreateModel CreateTestSubject(
        RazorBlogDbContext mockAppDbContext,
        UserManager<ApplicationUser> mockUserManager,
        TempDataDictionary tempData,
        PageContext pageContext,
        UrlHelper urlHelper)
    {
        return new CreateModel(
            mockAppDbContext,
            mockUserManager,
            _mockBlogContentManager.Object,
            _mockLogger.Object,
            _mockUserPermissionValidator.Object)
        {
            PageContext = pageContext,
            TempData = tempData,
            Url = urlHelper
        };
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    private async Task OnGetAsync_ShouldReturnForbid_IfUserIsNotFound(bool isUserNull)
    {
        await using var mockAppDbContext = await DatabaseTestUtil.CreateDbDummy();

        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
        var modelMetadataProvider = new EmptyModelMetadataProvider();

        var mockUserManager = UserManagerTestUtil.CreateUserManagerMock();
        var pageModel = CreateTestSubject(
            mockAppDbContext,
            UserManagerTestUtil.CreateUserManagerMock().Object,
            new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>()),
            new PageContext(actionContext) { ViewData = new ViewDataDictionary(modelMetadataProvider, modelState) },
            new UrlHelper(actionContext));

        mockUserManager
            .Setup(x => x.GetUserAsync(pageModel.User))
            .ReturnsAsync(isUserNull ? null : new ApplicationUser { UserName = null });

        var result = await pageModel.OnGetAsync();
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    private async Task OnGetAsync_ShouldReturnForbid_IfUserIsBanned()
    {
        var faker = new Faker();
        await using var mockAppDbContext = await DatabaseTestUtil.CreateDbDummy();

        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var mockUserManager = UserManagerTestUtil.CreateUserManagerMock();
        var pageModel = CreateTestSubject(
            mockAppDbContext,
            mockUserManager.Object,
            new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>()),
            new PageContext(actionContext) { ViewData = new ViewDataDictionary(modelMetadataProvider, modelState) },
            new UrlHelper(actionContext));

        var user = new ApplicationUser { UserName = faker.Name.LastName() };
        mockUserManager
            .Setup(x => x.GetUserAsync(pageModel.User))
            .ReturnsAsync(user);

        _mockUserPermissionValidator
            .Setup(x => x.IsUserAllowedToCreatePostAsync(user.UserName))
            .ReturnsAsync(false);

        var result = await pageModel.OnGetAsync();
        result.Should().BeOfType<ForbidResult>();
    }
}
