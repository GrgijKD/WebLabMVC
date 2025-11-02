using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebLabMVC.Models;

namespace WebLabMVC.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BooksController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

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
        public async Task<IActionResult> Create(Book book, IFormFile? coverFile, int[] Authors, int[] Genres)
        {
            if (!ModelState.IsValid)
            {
                PopulateSelectLists();
                return View(book);
            }

            if (coverFile != null && coverFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath ?? string.Empty, "images", "covers");
                Directory.CreateDirectory(uploads);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(coverFile.FileName)}";
                var filePath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await coverFile.CopyToAsync(stream);

                book.CoverUrl = $"/images/covers/{fileName}";
            }
            else
            {
                book.CoverUrl = "/images/covers/default.jpg";
            }

            if (!decimal.TryParse(book.Price.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var price)
                || price < 0.01M || price > 999999.99M)
            {
                ModelState.AddModelError("Price", "Ціна має бути від 0.01 до 999999.99");
                PopulateSelectLists();
                return View(book);
            }

            book.Price = price.ToString(CultureInfo.InvariantCulture);

            var selectedAuthors = Authors?.Length > 0
                ? await _context.Authors.Where(a => Authors.Contains(a.Id))
                    .Include(a => a.Genres)
                    .ToListAsync()
                : new List<Author>();
            book.Authors = selectedAuthors;

            var selectedGenres = Genres?.Length > 0
                ? await _context.Genres.Where(g => Genres.Contains(g.Id))
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

            _context.Add(book);
            await _context.SaveChangesAsync();
            await SyncAuthorGenres();

            return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> Edit(int id, Book book, IFormFile? coverFile, int[] Authors, int[] Genres)
        {
            if (id != book.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                PopulateSelectLists(book);
                return View(book);
            }

            var existingBook = await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.Genres)
                .Include(b => b.Publisher)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (existingBook == null)
                return NotFound();

            if (!decimal.TryParse(book.Price.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var price)
                || price < 0.01M || price > 999999.99M)
            {
                ModelState.AddModelError("Price", "Ціна має бути від 0.01 до 999999.99");
                PopulateSelectLists(book);
                return View(book);
            }

            existingBook.Price = price.ToString(CultureInfo.InvariantCulture);
            existingBook.Title = book.Title;
            existingBook.PublisherId = book.PublisherId;

            if (coverFile != null && coverFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath ?? string.Empty, "images", "covers");
                Directory.CreateDirectory(uploads);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(coverFile.FileName)}";
                var filePath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await coverFile.CopyToAsync(stream);

                existingBook.CoverUrl = $"/images/covers/{fileName}";
            }

            var selectedAuthors = Authors?.Length > 0
                ? await _context.Authors
                    .Where(a => Authors.Contains(a.Id))
                    .Include(a => a.Genres)
                    .ToListAsync()
                : new List<Author>();

            var selectedGenres = Genres?.Length > 0
                ? await _context.Genres
                    .Where(g => Genres.Contains(g.Id))
                    .Include(g => g.Authors)
                    .ToListAsync()
                : new List<Genre>();

            existingBook.Authors = selectedAuthors;
            existingBook.Genres = selectedGenres;

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
                if (!genre.Books.Contains(existingBook))
                    genre.Books.Add(existingBook);
            }

            foreach (var author in selectedAuthors)
            {
                if (!author.Books.Contains(existingBook))
                    author.Books.Add(existingBook);
            }

            await _context.SaveChangesAsync();
            await SyncAuthorGenres();

            return RedirectToAction(nameof(Index));
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
            await SyncAuthorGenres();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }

        private void PopulateSelectLists(Book? book = null)
        {
            book ??= new Book();

            ViewBag.Publishers = new SelectList(
                _context.Publishers.OrderBy(p => p.Name),
                "Id", "Name",
                book.PublisherId);

            ViewBag.Authors = new MultiSelectList(
                _context.Authors.OrderBy(a => a.FullName),
                "Id", "FullName",
                book.Authors?.Select(a => a.Id));

            ViewBag.Genres = new MultiSelectList(
                _context.Genres.OrderBy(g => g.Name),
                "Id", "Name",
                book.Genres?.Select(g => g.Id));
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
}