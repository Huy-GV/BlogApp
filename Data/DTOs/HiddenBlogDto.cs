namespace RazorBlog.Data.Dtos;

public class HiddenBlogDto
{
    public int Id { get; init; }
    public string Introduction { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public System.DateTime CreatedDate { get; init; }
}