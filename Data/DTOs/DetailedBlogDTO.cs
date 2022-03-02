using System;
using System.Collections.Generic;
using BlogApp.Models;

namespace BlogApp.Data.DTOs
{
    public class DetailedBlogDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string AuthorProfilePicture { get; set; }
        public string AuthorDescription { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public bool IsHidden { get; set; }
        public ICollection<CommentDto> CommentDtos { get; set; }
        public static DetailedBlogDto From(Blog blog)
        {
            List<CommentDto> commentDtos = new();
            foreach (var comment in blog.Comments)
            {
                commentDtos.Add(new CommentDto()
                {
                    Id = comment.ID,
                    Date = comment.Date,
                    AuthorName = comment.Author,
                    IsHidden = comment.IsHidden,
                    Content = comment.IsHidden ? comment.SuspensionExplanation : comment.Content,
                    AuthorProfilePicturePath = comment.AppUser?.ProfilePicturePath ?? "default.jpg"
                });
            }

            return new DetailedBlogDto()
            {
                Id = blog.ID,
                Title = blog.Title,
                AuthorName = blog.Author,
                AuthorDescription = blog.AppUser?.Description ?? string.Empty,
                AuthorProfilePicture = blog.AppUser?.ProfilePicturePath ?? "default.jpg",
                Description = blog.Description,
                IsHidden = blog.IsHidden,
                Content = blog.IsHidden ? blog.SuspensionExplanation : blog.Content,
                Date = blog.Date,
                CommentDtos = commentDtos
            };
        }
    }
}
