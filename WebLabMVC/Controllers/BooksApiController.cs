using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebLabMVC.Models;

[ApiController]
[Route("api/[controller]")]
public class BooksApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BooksApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Books
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetBook()
    {
        var books = await _context.Books
            .Include(b => b.Publisher)
            .Include(b => b.Authors)
            .Include(b => b.Genres)
            .Include(b => b.Shops)
            .ToListAsync();

        var dto = books.Select(b => new BookDto
        {
            Id = b.Id,
            Title = b.Title,
            Price = b.Price,
            CoverUrl = b.CoverUrl,
            PublisherId = b.PublisherId,
            Publisher = b.Publisher?.Name,
            AuthorIds = b.Authors?.Select(a => a.Id).ToArray(),
            GenreIds = b.Genres?.Select(g => g.Id).ToArray(),
            ShopIds = b.Shops?.Select(s => s.Id).ToArray(),
            Authors = b.Authors?.Select(a => a.FullName).ToList(),
            Genres = b.Genres?.Select(g => g.Name).ToList(),
            Shops = b.Shops?.Select(s => s.Name).ToList()
        });

        return Ok(dto);
    }

    // GET: api/Books/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBook(int id)
    {
        var book = await _context.Books
            .Include(b => b.Publisher)
            .Include(b => b.Authors)
            .Include(b => b.Genres)
            .Include(b => b.Shops)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
            return NotFound(new { message = "Книгу не знайдено" });

        var dto = new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Price = book.Price,
            CoverUrl = book.CoverUrl,
            PublisherId = book.PublisherId,
            Publisher = book.Publisher?.Name ?? "Не вказано",
            AuthorIds = book.Authors?.Select(a => a.Id).ToArray(),
            GenreIds = book.Genres?.Select(g => g.Id).ToArray(),
            ShopIds = book.Shops?.Select(s => s.Id).ToArray(),
            Authors = book.Authors?.Select(a => a.FullName).ToList(),
            Genres = book.Genres?.Select(g => g.Name).ToList(),
            Shops = book.Shops?.Select(s => s.Name).ToList()
        };

        return Ok(dto);
    }

    // POST: api/Books
    [HttpPost]
    public async Task<IActionResult> CreateBook([FromBody] BookDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { error = "Назва книги обов'язкова." });

        if (!decimal.TryParse(dto.Price.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var price)
            || price < 0.01M || price > 999999.99M)
        {
            return BadRequest(new { error = "Ціна має бути від 0.01 до 999999.99" });
        }

        var book = new Book
        {
            Title = dto.Title,
            Price = price.ToString(CultureInfo.InvariantCulture),
            CoverUrl = string.IsNullOrWhiteSpace(dto.CoverUrl)
                ? "/images/covers/default.jpg"
                : dto.CoverUrl,
            PublisherId = dto.PublisherId
        };

        var selectedAuthors = dto.AuthorIds?.Length > 0
            ? await _context.Authors
                .Where(a => dto.AuthorIds.Contains(a.Id))
                .Include(a => a.Genres)
                .ToListAsync()
            : new List<Author>();
        book.Authors = selectedAuthors;

        var selectedGenres = dto.GenreIds?.Length > 0
            ? await _context.Genres
                .Where(g => dto.GenreIds.Contains(g.Id))
                .Include(g => g.Authors)
                .ToListAsync()
            : new List<Genre>();
        book.Genres = selectedGenres;

        foreach (var author in selectedAuthors)
        {
            foreach (var genre in selectedGenres)
            {
                if (!author.Genres.Any(g => g.Id == genre.Id))
                    author.Genres.Add(genre);
            }
        }

        foreach (var genre in selectedGenres)
        {
            if (!genre.Books.Contains(book))
                genre.Books.Add(book);
        }

        foreach (var author in selectedAuthors)
        {
            if (!author.Books.Contains(book))
                author.Books.Add(book);
        }

        if (book.PublisherId != 0)
        {
            var publisher = await _context.Publishers
                .Include(p => p.Books)
                .FirstOrDefaultAsync(p => p.Id == book.PublisherId);
            if (publisher != null && !publisher.Books.Contains(book))
                publisher.Books.Add(book);
        }

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        await SyncAuthorGenres();

        return Ok(new { message = "Book created successfully", book.Id });
    }

    // PUT: api/Books/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBook(int id, [FromBody] BookDto bookDto)
    {
        var book = await _context.Books
            .Include(b => b.Authors)
            .Include(b => b.Genres)
            .Include(b => b.Publisher)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
            return NotFound(new { error = "Книгу не знайдено" });

        if (string.IsNullOrWhiteSpace(bookDto.Title))
            return BadRequest(new { error = "Назва книги обов'язкова." });

        if (!decimal.TryParse(bookDto.Price.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var price)
            || price < 0.01M || price > 999999.99M)
        {
            return BadRequest(new { error = "Ціна має бути від 0.01 до 999999.99" });
        }

        book.Title = bookDto.Title;
        book.Price = price.ToString(CultureInfo.InvariantCulture);
        book.PublisherId = bookDto.PublisherId;
        book.CoverUrl = string.IsNullOrWhiteSpace(bookDto.CoverUrl)
            ? book.CoverUrl ?? "/images/covers/default.jpg"
            : bookDto.CoverUrl;

        book.Authors.Clear();
        var selectedAuthors = bookDto.AuthorIds?.Length > 0
            ? await _context.Authors
                .Where(a => bookDto.AuthorIds.Contains(a.Id))
                .Include(a => a.Genres)
                .ToListAsync()
            : new List<Author>();
        foreach (var author in selectedAuthors)
            book.Authors.Add(author);

        book.Genres.Clear();
        var selectedGenres = bookDto.GenreIds?.Length > 0
            ? await _context.Genres
                .Where(g => bookDto.GenreIds.Contains(g.Id))
                .Include(g => g.Authors)
                .ToListAsync()
            : new List<Genre>();
        foreach (var genre in selectedGenres)
            book.Genres.Add(genre);

        foreach (var author in selectedAuthors)
        {
            foreach (var genre in selectedGenres)
            {
                if (!author.Genres.Any(g => g.Id == genre.Id))
                    author.Genres.Add(genre);
            }
        }

        foreach (var author in selectedAuthors)
        {
            if (!author.Books.Contains(book))
                author.Books.Add(book);
        }

        foreach (var genre in selectedGenres)
        {
            if (!genre.Books.Contains(book))
                genre.Books.Add(book);
        }

        if (book.PublisherId != 0)
        {
            var publisher = await _context.Publishers
                .Include(p => p.Books)
                .FirstOrDefaultAsync(p => p.Id == book.PublisherId);

            if (publisher != null && !publisher.Books.Contains(book))
                publisher.Books.Add(book);
        }

        await _context.SaveChangesAsync();
        await SyncAuthorGenres();

        return Ok(new { message = "Book updated successfully" });
    }

    // DELETE: api/Books/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null) return NotFound();

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        await SyncAuthorGenres();
        return Ok(new { message = "Book deleted" });
    }

    private async Task SyncAuthorGenres()
    {
        var authors = await _context.Authors
            .Include(a => a.Books)
                .ThenInclude(b => b.Genres)
            .Include(a => a.Genres)
            .ToListAsync();

        foreach (var author in authors)
        {
            var genresToRemove = author.Genres
                .Where(g => !author.Books.Any(b => b.Genres.Any(bg => bg.Id == g.Id)))
                .ToList();

            foreach (var genre in genresToRemove)
            {
                author.Genres.Remove(genre);
                genre.Authors.Remove(author);
            }
        }

        await _context.SaveChangesAsync();
    }
}