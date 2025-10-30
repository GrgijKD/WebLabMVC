using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebLabMVC.Models;

namespace WebLabMVC.Controllers
{
    public class ShopsController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        // GET: Shops
        public async Task<IActionResult> Index()
        {
            return View(await _context.Shops.ToListAsync());
        }

        // GET: Shops/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shop = await _context.Shops
                .FirstOrDefaultAsync(m => m.Id == id);
            if (shop == null)
            {
                return NotFound();
            }

            return View(shop);
        }

        // GET: Shops/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Shops/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address,Latitude,Longitude")] Shop shop)
        {
            if (ModelState.IsValid)
            {
                if (!decimal.TryParse(shop.Latitude.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var lat) || lat < -90M || lat > 90M)
                {
                    ModelState.AddModelError("Latitude", "Широта має бути від -90 до 90");
                    return View(shop);
                }

                if (!decimal.TryParse(shop.Longitude.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var lon) || lon < -180M || lon > 180M)
                {
                    ModelState.AddModelError("Longitude", "Довгота має бути від -180 до 180");
                    return View(shop);
                }

                shop.Latitude = lat.ToString(CultureInfo.InvariantCulture);
                shop.Longitude = lon.ToString(CultureInfo.InvariantCulture);

                _context.Add(shop);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(shop);
        }

        // GET: Shops/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shop = await _context.Shops.FindAsync(id);
            if (shop == null)
            {
                return NotFound();
            }
            return View(shop);
        }

        // POST: Shops/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Latitude,Longitude")] Shop shop)
        {
            if (id != shop.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!decimal.TryParse(shop.Latitude.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var lat) || lat < -90M || lat > 90M)
                    {
                        ModelState.AddModelError("Latitude", "Широта має бути від -90 до 90");
                        return View(shop);
                    }

                    if (!decimal.TryParse(shop.Longitude.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var lon) || lon < -180M || lon > 180M)
                    {
                        ModelState.AddModelError("Longitude", "Довгота має бути від -180 до 180");
                        return View(shop);
                    }

                    shop.Latitude = lat.ToString(CultureInfo.InvariantCulture);
                    shop.Longitude = lon.ToString(CultureInfo.InvariantCulture);

                    _context.Update(shop);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShopExists(shop.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(shop);
        }

        // GET: Shops/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shop = await _context.Shops
                .FirstOrDefaultAsync(m => m.Id == id);
            if (shop == null)
            {
                return NotFound();
            }

            return View(shop);
        }

        // POST: Shops/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var shop = await _context.Shops.FindAsync(id);
            if (shop != null)
            {
                _context.Shops.Remove(shop);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ShopExists(int id)
        {
            return _context.Shops.Any(e => e.Id == id);
        }
    }
}
