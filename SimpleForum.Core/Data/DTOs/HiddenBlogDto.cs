namespace SimpleForum.Core.Data.Dtos;

public record HiddenBlogDto : HiddenPostPto
{
    public required string Introduction { get; init; }
    public required string Title { get; init; }
}
