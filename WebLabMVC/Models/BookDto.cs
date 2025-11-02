public class BookDto
{
    public int Id { get; set; }

    public string Title { get; set; }
    public string Price { get; set; }
    public string? CoverUrl { get; set; }

    public int? PublisherId { get; set; }
    public string? Publisher { get; set; }

    public int[]? AuthorIds { get; set; }
    public int[]? GenreIds { get; set; }
    public int[]? ShopIds { get; set; }

    public List<string>? Authors { get; set; }
    public List<string>? Genres { get; set; }
    public List<string>? Shops { get; set; }
}