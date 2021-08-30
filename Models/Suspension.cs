using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Models 
{
    public class Suspension
    {
        public int ID { get; set; }
        [Required]
        public string Username { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Expiry { get; set;}
    }
}