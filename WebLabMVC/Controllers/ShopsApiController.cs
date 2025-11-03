using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebLabMVC.Models;

[ApiController]
[Route("api/[controller]")]
public class ShopsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public ShopsApiController(ApplicationDbContext context) => _context = context;

    // GET: api/Shops
    [HttpGet]
    public async Task<IActionResult> GetShops()
    {
        var shops = await _context.Shops
            .Include(s => s.Books)
            .ToListAsync();

        var dtoList = shops.Select(s => new ShopDto
        {
            Id = s.Id,
            Name = s.Name,
            Address = s.Address,
            Latitude = s.Latitude,
            Longitude = s.Longitude,
            Books = s.Books?.Select(b => new BookShortDto
            {
                Id = b.Id,
                Title = b.Title
            }).ToList()
        }).ToList();

        return Ok(dtoList);
    }

    // GET: api/Shops/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetShop(int id)
    {
        var shop = await _context.Shops
            .Include(s => s.Books)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (shop == null)
            return NotFound(new { message = "Магазин не знайдено" });

        var dto = new ShopDto
        {
            Id = shop.Id,
            Name = shop.Name,
            Address = shop.Address,
            Latitude = shop.Latitude,
            Longitude = shop.Longitude,
            Books = shop.Books?.Select(b => new BookShortDto
            {
                Id = b.Id,
                Title = b.Title
            }).ToList()
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateShop([FromBody] ShopDto dto)
    {
        var shop = new Shop
        {
            Name = dto.Name,
            Address = dto.Address,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude
        };

        _context.Shops.Add(shop);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Shop created", shop.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateShop(int id, [FromBody] ShopDto dto)
    {
        var shop = await _context.Shops.Include(s => s.Books).FirstOrDefaultAsync(s => s.Id == id);
        if (shop == null) return NotFound();

        shop.Name = dto.Name;
        shop.Latitude = dto.Latitude;
        shop.Longitude = dto.Longitude;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Shop updated" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteShop(int id)
    {
        var shop = await _context.Shops.FindAsync(id);
        if (shop == null) return NotFound();
        _context.Shops.Remove(shop);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Shop deleted" });
    }
}