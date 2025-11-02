namespace WebLabMVC.Models
{
    public class PublisherDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public List<BookShortDto>? Books { get; set; }
    }
}