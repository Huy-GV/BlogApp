namespace RazorBlog.Data.DTOs;

public class HiddenBlogDto
{
    public int Id { get; init; }
    public string Introduction { get; init; }
    public string Content { get; init; }
    public string Title { get; init; }
    public System.DateTime CreatedDate { get; init; }
}