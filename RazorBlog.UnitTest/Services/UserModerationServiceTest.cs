using Bogus;
using FluentAssertions;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using RazorBlog.Core.Data;
using RazorBlog.Core.Models;
using RazorBlog.Core.Services;
using RazorBlog.UnitTest.Utils;

namespace RazorBlog.UnitTest.Services;

public class UserModerationServiceTest
{
    private readonly Mock<IUserPermissionValidator> _mockUserPermissionValidator = new();
    private readonly Mock<ILogger<UserModerationService>> _mockLogger = new();
    private readonly Mock<IBackgroundJobClient> _mockHangfireClient = new();

    private IUserModerationService CreateTestSubject(
        RazorBlogDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        return new UserModerationService(dbContext,
            _mockLogger.Object,
            userManager,
            _mockHangfireClient.Object);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    private async Task BanUserAsync_ShouldFail_IfUserIsNotAdmin(bool isUserFound, bool isAdminRole)
    {
        var faker = new Faker();
        var userToBanName = faker.Name.LastName();
        var banningUserName = faker.Name.LastName();
        await using var dbContext = await DatabaseTestUtil.CreateDbDummy();

        var mockUserManager = UserManagerTestUtil.CreateUserManagerMock();
        if (isUserFound)
        {
            var user = new ApplicationUser { UserName = banningUserName };
            mockUserManager
                .Setup(x => x.FindByNameAsync(banningUserName))
                .ReturnsAsync(user);

            mockUserManager
                .Setup(x => x.IsInRoleAsync(user, "admin"))
                .ReturnsAsync(isAdminRole);
        }

        var userModerationService = CreateTestSubject(dbContext, mockUserManager.Object);

        var result = await userModerationService.BanUserAsync(
            userToBanName,
            banningUserName,
            It.IsAny<DateTime?>());

        result.Should().Be(Core.Communication.ServiceResultCode.Unauthorized);
    }

    [Fact]
    private async Task BanUserAsync_ShouldFail_IfExpiryIsInThePast()
    {
        var faker = new Faker();
        var userToBanName = faker.Name.LastName();
        var banningUserName = faker.Name.LastName();
        await using var dbContext = await DatabaseTestUtil.CreateDbDummy();
        var mockUserManager = UserManagerTestUtil.CreateUserManagerMock();
        var user = new ApplicationUser { UserName = banningUserName };
        mockUserManager
            .Setup(x => x.FindByNameAsync(banningUserName))
            .ReturnsAsync(user);

        mockUserManager
            .Setup(x => x.IsInRoleAsync(user, "admin"))
            .ReturnsAsync(true);

        var userModerationService = CreateTestSubject(dbContext, mockUserManager.Object);

        var result = await userModerationService.BanUserAsync(
            userToBanName,
            banningUserName,
            faker.Date.Past());

        result.Should().Be(Core.Communication.ServiceResultCode.InvalidArguments);
    }
}