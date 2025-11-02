using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebLabMVC.Models
{
    public class Shop
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Заповніть це поле")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Заповніть це поле")]
        public required string Address { get; set; }

        [Required(ErrorMessage = "Заповніть це поле")]
        public required string Latitude { get; set; }

        [Required(ErrorMessage = "Заповніть це поле")]
        public required string Longitude { get; set; }

        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}