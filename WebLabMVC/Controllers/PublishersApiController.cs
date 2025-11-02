using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebLabMVC.Models;

[ApiController]
[Route("api/[controller]")]
public class PublishersApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public PublishersApiController(ApplicationDbContext context) => _context = context;

    // GET: api/Publishers
    [HttpGet]
    public async Task<IActionResult> GetPublishers()
    {
        var publishers = await _context.Publishers
            .Include(p => p.Books)
            .ToListAsync();

        var dtoList = publishers.Select(p => new PublisherDto
        {
            Id = p.Id,
            Name = p.Name,
            Country = p.Country,
            Books = p.Books.Select(b => new BookShortDto { Id = b.Id, Title = b.Title }).ToList()
        });

        return Ok(dtoList);
    }

    // GET: api/Publishers/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPublisher(int id)
    {
        var publisher = await _context.Publishers
            .Include(p => p.Books)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (publisher == null)
            return NotFound(new { message = "Видавництво не знайдено" });

        var dto = new PublisherDto
        {
            Id = publisher.Id,
            Name = publisher.Name,
            Country = publisher.Country,
            Books = publisher.Books.Select(b => new BookShortDto { Id = b.Id, Title = b.Title }).ToList()
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePublisher([FromBody] PublisherDto dto)
    {
        var publisher = new Publisher { Name = dto.Name, Country = dto.Country };

        _context.Publishers.Add(publisher);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Publisher created", publisher.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePublisher(int id, [FromBody] PublisherDto dto)
    {
        var publisher = await _context.Publishers.Include(p => p.Books).FirstOrDefaultAsync(p => p.Id == id);
        if (publisher == null) return NotFound();

        publisher.Name = dto.Name;
        publisher.Country = dto.Country;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Publisher updated" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePublisher(int id)
    {
        var publisher = await _context.Publishers.FindAsync(id);
        if (publisher == null) return NotFound();
        _context.Publishers.Remove(publisher);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Publisher deleted" });
    }
}