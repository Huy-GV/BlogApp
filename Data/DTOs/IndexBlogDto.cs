using RazorBlog.Data.DTOs;

namespace RazorBlog.Data.Dtos;

public record IndexBlogDto : PostPto
{
    public required string Title { get; set; }
    public required uint ViewCount { get; set; }
    public required string Introduction { get; set; }
    public required string CoverImageUri { get; set; }
}
