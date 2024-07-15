using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using RazorBlog.Core.Data;
using RazorBlog.Core.Data.ViewModels;
using RazorBlog.Core.Models;
using RazorBlog.Core.ReadServices;
using RazorBlog.Core.WriteServices;
using RazorBlog.UnitTest.Utils;

namespace RazorBlog.UnitTest.Services;

public class CommentContentManagerTest
{
    private readonly Mock<IUserPermissionValidator> _mockUserPermissionValidator = new();
    private readonly Mock<IBanTicketReader> _mockBanTicketReader = new();

    private ICommentContentManager CreateTestSubject(
        RazorBlogDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        return new CommentContentManager(dbContext,
            _mockBanTicketReader.Object,
            _mockUserPermissionValidator.Object,
            userManager);
    }

    [Fact]
    private async Task CreateComment_ShouldFail_IfUserIsBanned()
    {
        var faker = new Faker();
        var mockUserManager = UserManagerTestUtil.CreateUserManagerMock();

        var bannedUser = new ApplicationUser()
        {
            UserName = faker.Name.LastName(),
        };

        mockUserManager
            .Setup(x => x.FindByNameAsync(bannedUser.UserName))
            .ReturnsAsync(bannedUser);

        _mockUserPermissionValidator
            .Setup(x => x.IsUserAllowedToCreatePostAsync(bannedUser.UserName))
            .ReturnsAsync(false);

        await using var dbContext = await DatabaseTestUtil.CreateDbDummy();
        var commentContentManager = CreateTestSubject(
                dbContext,
                mockUserManager.Object);

        var viewModel = new CommentViewModel();

        var (code, id) = await commentContentManager.CreateCommentAsync(viewModel, bannedUser.UserName);
        code.Should().Be(Core.Communication.ServiceResultCode.Unauthorized);
        id.Should().BeNull();
    }

    [Fact]
    private async Task CreateComment_ShouldFail_IfUserIsNotFound()
    {
        var faker = new Faker();
        var bannedUserName = faker.Name.LastName();
        var mockUserManager = UserManagerTestUtil.CreateUserManagerMock();
        var bannedUser = new ApplicationUser()
        {
            UserName = faker.Name.LastName(),
        };

        mockUserManager
            .Setup(x => x.FindByNameAsync(bannedUser.UserName))
            .ReturnsAsync(bannedUser);

        _mockUserPermissionValidator
            .Setup(x => x.IsUserAllowedToCreatePostAsync(bannedUserName))
            .ReturnsAsync(false);

        await using var dbContext = await DatabaseTestUtil.CreateDbDummy();
        var commentContentManager = CreateTestSubject(
                dbContext,
                mockUserManager.Object);

        var viewModel = new CommentViewModel();

        var (code, id) = await commentContentManager.CreateCommentAsync(viewModel, bannedUserName);
        code.Should().Be(Core.Communication.ServiceResultCode.Unauthorized);
        id.Should().BeNull();
    }

    [Fact]
    private async Task CreateComment_ShouldSucceed()
    {
        var faker = new Faker();
        var mockUserManager = UserManagerTestUtil.CreateUserManagerMock();
        var user = new ApplicationUser()
        {
            UserName = faker.Name.LastName()
        };

        var blog = new Blog
        {
            Title = faker.Lorem.Sentence(10),
            Body = faker.Lorem.Sentences(3),
            CoverImageUri = faker.Lorem.Sentence(10),
            AuthorUserName = user.UserName
        };

        mockUserManager
           .Setup(x => x.FindByNameAsync(user.UserName))
           .ReturnsAsync(user);

        _mockUserPermissionValidator
            .Setup(x => x.IsUserAllowedToCreatePostAsync(user.UserName))
            .ReturnsAsync(true);

        await using var dbContext = await DatabaseTestUtil.CreateInMemorySqliteDbMock();

        dbContext.Users.Add(user);
        dbContext.Blog.Add(blog);
        await dbContext.SaveChangesAsync();

        var viewModel = new CommentViewModel()
        {
            BlogId = blog.Id,
            Body = faker.Lorem.Sentence(),
        };

        var commentContentManager = CreateTestSubject(
                dbContext,
                mockUserManager.Object);

        var (code, id) = await commentContentManager.CreateCommentAsync(viewModel, user.UserName);
        code.Should().Be(Core.Communication.ServiceResultCode.Success);
        var addedComment = await dbContext.Comment.FirstAsync(x => x.Id == id);
        addedComment
            .Should()
            .BeEquivalentTo(new Comment
            {
                Id = id!.Value,
                BlogId = viewModel.BlogId,
                Body = viewModel.Body,
                AuthorUserName = user.UserName
            },
            options => options
                .Excluding(x => x.CreationTime)
                .Excluding(x => x.LastUpdateTime)
                .Excluding(x => x.AuthorUser));
    }
}
