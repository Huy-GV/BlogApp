using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RazorBlog.Core.Communication;
using RazorBlog.Core.ReadServices;

namespace RazorBlog.UnitTest.Services;

public class AggregateImageUriResolverTest
{
    private readonly Mock<ILogger<AggregateImageUriResolver>> _mockLogger = new();

    [Fact]
    private async Task ResolveImageUriAsync_ShouldReturnNull_IfUriIsEmpty()
    {
        var aggregateResolver = new AggregateImageUriResolver(_mockLogger.Object, [It.IsAny<IImageUriResolver>()]);
        var result = await aggregateResolver.ResolveImageUriAsync(string.Empty);
        result.Should().BeNull();
    }

    [Fact]
    private async Task ResolveImageUriAsync_ShouldReturnNull_IfUriIsNotResolved()
    {
        var faker = new Faker();
        var failureCodes = Enum.GetValues<ServiceResultCode>().Where(x => x != ServiceResultCode.Success);

        var originalImageUri = faker.Internet.Url();

        var invalidResolvers = new List<IImageUriResolver>();
        for (var i = 0; i < 10; i++) {
            var mockInvalidImageResolver = new Mock<IImageUriResolver>();
            mockInvalidImageResolver
                .Setup(x => x.ResolveImageUri(originalImageUri))
                .ReturnsAsync((faker.PickRandom(failureCodes), null));

            invalidResolvers.Add(mockInvalidImageResolver.Object);
        }

        var aggregateResolver = new AggregateImageUriResolver(
            _mockLogger.Object,
            invalidResolvers);

        var result = await aggregateResolver.ResolveImageUriAsync(originalImageUri);
        result.Should().BeNull();
    }

    [Fact]
    private async Task ResolveImageUriAsync_ShouldReturnFirstSuccessfulResult()
    {
        var faker = new Faker();
        var originalImageUri = faker.Internet.Url();
        var resolvedImageUri = faker.Internet.Url();

        var mockInvalidImageResolver1 = new Mock<IImageUriResolver>();
        mockInvalidImageResolver1
            .Setup(x => x.ResolveImageUri(originalImageUri))
            .ReturnsAsync((ServiceResultCode.InvalidArguments, null));

        var mockInvalidImageResolver2 = new Mock<IImageUriResolver>();
        mockInvalidImageResolver2
            .Setup(x => x.ResolveImageUri(originalImageUri))
            .ReturnsAsync(It.IsAny<(ServiceResultCode, string?)>());


        var mockImageResolver = new Mock<IImageUriResolver>();
        mockImageResolver
            .Setup(x => x.ResolveImageUri(originalImageUri))
            .ReturnsAsync((ServiceResultCode.Success, resolvedImageUri));

        var aggregateResolver = new AggregateImageUriResolver(
            _mockLogger.Object,
            [
                mockInvalidImageResolver1.Object,
                mockImageResolver.Object,
                mockInvalidImageResolver2.Object,
            ]);

        var result = await aggregateResolver.ResolveImageUriAsync(originalImageUri);
        result.Should().BeEquivalentTo(resolvedImageUri);
        mockInvalidImageResolver2.Verify(x => x.ResolveImageUri(originalImageUri), Times.Never);
    }
}
