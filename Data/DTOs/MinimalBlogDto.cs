namespace RazorBlog.Data.Dtos;

public record MinimalBlogDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public System.DateTime Date { get; set; }
    public uint ViewCount { get; set; }
}