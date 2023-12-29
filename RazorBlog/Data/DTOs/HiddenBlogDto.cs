using RazorBlog.Data.Constants;
using RazorBlog.Data.DTOs;

namespace RazorBlog.Data.Dtos;

public record HiddenBlogDto : HiddenPostPto
{
    public required string Introduction { get; init; }
    public required string Title { get; init; }
}