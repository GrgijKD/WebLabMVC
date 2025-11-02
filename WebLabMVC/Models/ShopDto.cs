namespace WebLabMVC.Models
{
    public class ShopDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Address { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public List<BookShortDto>? Books { get; set; }
    }
}