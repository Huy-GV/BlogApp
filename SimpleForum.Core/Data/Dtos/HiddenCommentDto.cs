namespace SimpleForum.Core.Data.Dtos;

public record HiddenCommentDto : HiddenPostPto
{
    public required int ThreadId { get; init; }
}
