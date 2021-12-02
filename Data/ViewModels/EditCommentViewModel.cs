  using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BlogApp.Data.ViewModel
{  
    public class EditCommentViewModel
    {
        public string Content { get; set; }
    }
}