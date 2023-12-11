namespace RazorBlog.Data.Dtos;

public class HiddenCommentDto
{
    public required int Id { get; init; }
    public required string Content { get; init; }
    public required System.DateTime CreationTime { get; init; }
}