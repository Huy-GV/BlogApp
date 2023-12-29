using RazorBlog.Data.Constants;

namespace RazorBlog.Data.DTOs;

public record HiddenPostPto
{
    public required int Id { get; init; }
    public required string Content { get; init; }
    public required System.DateTime CreationTime { get; init; }
}
