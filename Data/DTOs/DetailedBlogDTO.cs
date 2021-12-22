using System;
using System.Collections.Generic;
using BlogApp.Models;

namespace BlogApp.Data.DTOs
{
    public class DetailedBlogDTO
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string AuthorProfilePicture { get; set; }
        public string AuthorDescription { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public bool IsHidden { get; set; }
        public ICollection<CommentDTO> CommentDTOs { get; set; }
        public static DetailedBlogDTO From(Blog blog)
        {
            List<CommentDTO> commentDTOs = new();
            foreach (var comment in blog.Comments)
            {
                commentDTOs.Add(new CommentDTO()
                {
                    ID = comment.ID,
                    Date = comment.Date,
                    AuthorName = comment.Author,
                    IsHidden = comment.IsHidden,
                    Content = comment.IsHidden ? comment.SuspensionExplanation : comment.Content,
                    AuthorProfilePicture = comment.AppUser?.ProfilePicture ?? "default.jpg"
                });
            }

            return new DetailedBlogDTO()
            {
                ID = blog.ID,
                Title = blog.Title,
                AuthorName = blog.Author,
                AuthorDescription = blog.AppUser?.Description ?? string.Empty,
                AuthorProfilePicture = blog.AppUser?.ProfilePicture ?? "default.jpg",
                Description = blog.Description,
                IsHidden = blog.IsHidden,
                Content = blog.IsHidden ? blog.SuspensionExplanation : blog.Content,
                Date = blog.Date,
                CommentDTOs = commentDTOs
            };
        }
    }
}
