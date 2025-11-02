namespace WebLabMVC.Models
{
    public class GenreDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<BookShortDto>? Books { get; set; }
        public List<AuthorShortDto>? Authors { get; set; }
    }
}