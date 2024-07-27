namespace SimpleForum.Core.Data.Dtos;

public record HiddenThreadDto : HiddenPostPto
{
    public required string Title { get; init; }
}
