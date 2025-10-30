using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebLabMVC.Models
{
    public class Author
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Заповніть це поле")]
        public required string FullName { get; set; }

        public string? Country { get; set; }

        public ICollection<Genre> Genres { get; set; } = [];
        public ICollection<Book> Books { get; set; } = [];
    }
}