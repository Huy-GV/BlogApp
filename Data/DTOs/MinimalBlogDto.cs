namespace RazorBlog.Data.Dtos;

public record MinimalBlogDto
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public System.DateTime CreationTime { get; set; }
    public required uint ViewCount { get; set; }
}