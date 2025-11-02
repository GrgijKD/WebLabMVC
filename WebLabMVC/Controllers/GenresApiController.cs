using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebLabMVC.Models;

[ApiController]
[Route("api/[controller]")]
public class GenresApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public GenresApiController(ApplicationDbContext context) => _context = context;

    // GET: api/Genres
    [HttpGet]
    public async Task<IActionResult> GetGenres()
    {
        var genres = await _context.Genres
            .Include(g => g.Books)
            .Include(g => g.Authors)
            .ToListAsync();

        var dtoList = genres.Select(g => new GenreDto
        {
            Id = g.Id,
            Name = g.Name,
            Books = g.Books.Select(b => new BookShortDto { Id = b.Id, Title = b.Title }).ToList(),
            Authors = g.Authors.Select(a => new AuthorShortDto { Id = a.Id, FullName = a.FullName }).ToList()
        });

        return Ok(dtoList);
    }

    // GET: api/Genres/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetGenre(int id)
    {
        var genre = await _context.Genres
            .Include(g => g.Books)
            .Include(g => g.Authors)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (genre == null)
            return NotFound(new { message = "Жанр не знайдено" });

        var dto = new GenreDto
        {
            Id = genre.Id,
            Name = genre.Name,
            Books = genre.Books.Select(b => new BookShortDto { Id = b.Id, Title = b.Title }).ToList(),
            Authors = genre.Authors.Select(a => new AuthorShortDto { Id = a.Id, FullName = a.FullName }).ToList()
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateGenre([FromBody] GenreDto dto)
    {
        var genre = new Genre { Name = dto.Name };

        _context.Genres.Add(genre);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Genre created", genre.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGenre(int id, [FromBody] GenreDto dto)
    {
        var genre = await _context.Genres.Include(g => g.Authors).Include(g => g.Books)
            .FirstOrDefaultAsync(g => g.Id == id);
        if (genre == null) return NotFound();

        genre.Name = dto.Name;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Genre updated" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGenre(int id)
    {
        var genre = await _context.Genres.FindAsync(id);
        if (genre == null) return NotFound();
        _context.Genres.Remove(genre);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Genre deleted" });
    }
}