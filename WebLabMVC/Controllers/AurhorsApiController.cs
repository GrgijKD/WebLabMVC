using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebLabMVC.Models;

[ApiController]
[Route("api/[controller]")]
public class AuthorsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public AuthorsApiController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors()
    {
        var authors = await _context.Authors
            .Include(a => a.Books)
            .Include(a => a.Genres)
            .ToListAsync();

        var dto = authors.Select(a => new AuthorDto
        {
            Id = a.Id,
            FullName = a.FullName,
            Country = a.Country,
            Books = a.Books.Select(b => new BookShortDto
            {
                Id = b.Id,
                Title = b.Title
            }).ToList(),
            Genres = a.Genres.Select(g => new GenreShortDto
            {
                Id = g.Id,
                Name = g.Name
            }).ToList()
        });

        return Ok(dto);
    }

    // GET: api/Authors/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAuthor(int id)
    {
        var author = await _context.Authors
            .Include(a => a.Books)
            .Include(a => a.Genres)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (author == null)
            return NotFound(new { message = "Автора не знайдено" });

        var dto = new AuthorDto
        {
            Id = author.Id,
            FullName = author.FullName,
            Country = author.Country,
            Books = author.Books.Select(b => new BookShortDto
            {
                Id = b.Id,
                Title = b.Title
            }).ToList(),
            Genres = author.Genres.Select(g => new GenreShortDto
            {
                Id = g.Id,
                Name = g.Name
            }).ToList()
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAuthor([FromBody] AuthorDto dto)
    {
        var author = new Author { FullName = dto.FullName, Country = dto.Country };
        _context.Authors.Add(author);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Author created", author.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAuthor(int id, [FromBody] AuthorDto dto)
    {
        var author = await _context.Authors
            .Include(a => a.Genres)
            .Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (author == null) return NotFound();

        author.FullName = dto.FullName;
        author.Country = dto.Country;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Author updated" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuthor(int id)
    {
        var author = await _context.Authors.FindAsync(id);
        if (author == null) return NotFound();
        _context.Authors.Remove(author);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Author deleted" });
    }
}