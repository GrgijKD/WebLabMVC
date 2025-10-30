using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebLabMVC.Models
{
    public class Publisher
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Заповніть це поле")]
        public required string Name { get; set; }

        public string? Country { get; set; }

        public ICollection<Book> Books { get; set; } = [];
    }
}