using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WebLabMVC.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Заповніть це поле")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Ціна має бути від 0.01 до 999999.99")]
        public string Price { get; set; }

        public string? CoverUrl { get; set; }
        
        public int? PublisherId { get; set; }

        public Publisher? Publisher { get; set; }

        public ICollection<Author>? Authors { get; set; } = new List<Author>();
        public ICollection<Genre>? Genres { get; set; } = new List<Genre>();
        public ICollection<Shop>? Shops { get; set; } = new List<Shop>();
    }
}