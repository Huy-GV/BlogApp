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
        public string Description { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
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
                    Content = comment.IsHidden ? comment.SuspensionExplanation : comment.Content,
                    AuthorProfilePicture = comment.AppUser?.ProfilePicture ?? "default"
                });
            }

            return new DetailedBlogDTO()
            {
                ID = blog.ID,
                Title = blog.Title,
                AuthorName = blog.Author,
                AuthorProfilePicture= blog.AppUser?.ProfilePicture ?? "default",
                Description = blog.Description,
                Content = blog.IsHidden ? blog.SuspensionExplanation : blog.Content,
                Date = blog.Date,
                CommentDTOs = commentDTOs
            };
        }
    }
}
