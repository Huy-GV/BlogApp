  using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BlogApp.Data.FormModels
{  
    public class EditComment
    {
        public string Content { get; set; }
    }
}