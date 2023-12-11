namespace RazorBlog.Data.Dtos;

public class HiddenBlogDto
{
    public required int Id { get; init; }
    public required string Introduction { get; init; } = string.Empty;
    public required string Content { get; init; } = string.Empty;
    public required string Title { get; init; } = string.Empty;
    public required System.DateTime CreationTime { get; init; }
}