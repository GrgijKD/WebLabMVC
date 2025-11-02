using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebLabMVC.Models
{
    public class Genre
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Заповніть це поле")]
        public required string Name { get; set; }

        public ICollection<Book> Books { get; set; } = new List<Book>();
        public ICollection<Author> Authors { get; set; } = new List<Author>();
    }
}