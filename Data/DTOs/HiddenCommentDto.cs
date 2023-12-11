namespace RazorBlog.Data.Dtos;

public class HiddenCommentDto
{
    public int Id { get; init; }
    public string Content { get; init; } = string.Empty;
    public System.DateTime CreatedDate { get; init; }
}