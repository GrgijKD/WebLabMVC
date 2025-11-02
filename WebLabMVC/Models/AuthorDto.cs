namespace WebLabMVC.Models
{
    public class AuthorDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Country { get; set; }

        public List<BookShortDto>? Books { get; set; }
        public List<GenreShortDto>? Genres { get; set; }
    }
}