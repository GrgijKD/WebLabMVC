using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebLabMVC.Models;

namespace WebLabMVC.Controllers
{
    public class BooksController(ApplicationDbContext context, IWebHostEnvironment env) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IWebHostEnvironment _env = env;

        // GET: Books
        public async Task<IActionResult> Index()
        {
            var books = await _context.Books
                .Include(b => b.Publisher)
                .Include(b => b.Authors)
                .Include(b => b.Genres)
                .ToListAsync();
            return View(books);
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .Include(b => b.Publisher)
                .Include(b => b.Authors)
                .Include(b => b.Genres)
                .Include(b => b.Shops)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null) return NotFound();
            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            PopulateSelectLists();
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book, IFormFile coverFile, int[] selectedAuthors, int[] selectedGenres)
        {
            if (string.IsNullOrWhiteSpace(book.Title))
            {
                ModelState.AddModelError("Title", "Заповніть це поле.");
            }

            if (book.PublisherId == 0) book.PublisherId = 0;
            if (book.Authors == null || book.Authors.Count == 0) book.Authors = [];
            if (book.Genres == null || book.Genres.Count == 0) book.Genres = [];

            if (ModelState.IsValid)
            {
                // Cover
                if (coverFile != null && coverFile.Length > 0)
                {
                    var uploads = Path.Combine(_env.WebRootPath, "images/covers");
                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
                    var fileName = Path.GetFileName(coverFile.FileName);
                    var filePath = Path.Combine(uploads, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await coverFile.CopyToAsync(stream);
                    }
                    book.CoverUrl = "/images/covers/" + fileName;
                }
                else
                {
                    book.CoverUrl = "/images/covers/default.jpg";
                }

                // Authors, genres
                if (selectedAuthors != null)
                {
                    book.Authors = await _context.Authors
                        .Where(a => selectedAuthors.Contains(a.Id))
                        .ToListAsync();
                }

                if (selectedGenres != null)
                {
                    book.Genres = await _context.Genres
                        .Where(g => selectedGenres.Contains(g.Id))
                        .ToListAsync();
                }

                // Publisher
                if (book.PublisherId == 0)
                {
                    var defaultPublisher = await _context.Publishers
                        .FirstOrDefaultAsync(p => p.Name == "Не вказано");
                    if (defaultPublisher == null)
                    {
                        defaultPublisher = new Publisher { Name = "Не вказано", Country = "Не вказано" };
                        _context.Publishers.Add(defaultPublisher);
                        await _context.SaveChangesAsync();
                    }
                    book.PublisherId = defaultPublisher.Id;
                }

                if (!decimal.TryParse(book.Price.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var price) || price < 0.01M || price > 999999.99M )
                {
                    ModelState.AddModelError("Price", "Ціна має бути від 0.01 до 999999.99");
                    return View(book);
                }

                book.Price = price.ToString(CultureInfo.InvariantCulture);

                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PopulateSelectLists();
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.Genres)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return NotFound();

            PopulateSelectLists(book);
            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book, IFormFile coverFile, int[] selectedAuthors, int[] selectedGenres)
        {
            if (id != book.Id) return NotFound();

            if (string.IsNullOrWhiteSpace(book.Title))
            {
                ModelState.AddModelError("Title", "Назва книги обов'язкова.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var bookToUpdate = await _context.Books
                        .Include(b => b.Authors)
                        .Include(b => b.Genres)
                        .FirstOrDefaultAsync(b => b.Id == id);

                    if (bookToUpdate == null) return NotFound();

                    bookToUpdate.Title = book.Title;
                    bookToUpdate.Price = book.Price;

                    // Publisher
                    if (book.PublisherId == 0)
                    {
                        var defaultPublisher = await _context.Publishers
                            .FirstOrDefaultAsync(p => p.Name == "Не вказано");
                        if (defaultPublisher == null)
                        {
                            defaultPublisher = new Publisher { Name = "Не вказано", Country = "Не вказано" };
                            _context.Publishers.Add(defaultPublisher);
                            await _context.SaveChangesAsync();
                        }
                        bookToUpdate.PublisherId = defaultPublisher.Id;
                    }
                    else
                    {
                        bookToUpdate.PublisherId = book.PublisherId;
                    }

                    // Authors, genres
                    bookToUpdate.Authors.Clear();
                    if (selectedAuthors != null)
                    {
                        var authors = await _context.Authors
                            .Where(a => selectedAuthors.Contains(a.Id))
                            .ToListAsync();
                        foreach (var a in authors) bookToUpdate.Authors.Add(a);
                    }

                    bookToUpdate.Genres.Clear();
                    if (selectedGenres != null)
                    {
                        var genres = await _context.Genres
                            .Where(g => selectedGenres.Contains(g.Id))
                            .ToListAsync();
                        foreach (var g in genres) bookToUpdate.Genres.Add(g);
                    }

                    // Cover
                    if (coverFile != null && coverFile.Length > 0)
                    {
                        var uploads = Path.Combine(_env.WebRootPath, "images/covers");
                        if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
                        var fileName = Path.GetFileName(coverFile.FileName);
                        var filePath = Path.Combine(uploads, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await coverFile.CopyToAsync(stream);
                        }
                        bookToUpdate.CoverUrl = "/images/covers/" + fileName;
                    }

                    if (!decimal.TryParse(book.Price.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var price) || price < 0.01M || price > 999999.99M)
                    {
                        ModelState.AddModelError("Price", "Ціна має бути від 0.01 до 999999.99");
                        return View(book);
                    }

                    book.Price = price.ToString(CultureInfo.InvariantCulture);

                    _context.Update(bookToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            PopulateSelectLists(book);
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .Include(b => b.Publisher)
                .Include(b => b.Authors)
                .Include(b => b.Genres)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null) return NotFound();
            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }

        private void PopulateSelectLists(Book? book = null)
        {
            ViewBag.Authors = new MultiSelectList(_context.Authors.OrderBy(a => a.FullName), "Id", "FullName",
                book?.Authors.Select(a => a.Id));
            ViewBag.Genres = new MultiSelectList(_context.Genres.OrderBy(g => g.Name), "Id", "Name",
                book?.Genres.Select(g => g.Id));
            ViewBag.Publishers = new SelectList(_context.Publishers.OrderBy(p => p.Name), "Id", "Name",
                book?.PublisherId);
        }
    }
}