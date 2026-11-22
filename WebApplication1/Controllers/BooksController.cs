using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class BooksController : Controller
{
    private readonly LibraryContext _context;
    private const int PageSize = 6; // V1 (A1): 6 carti pe pagina

    public BooksController(LibraryContext context)
    {
        _context = context;
    }

    // GET: Books
    public async Task<IActionResult> Index(
        string? sortOrder,
        string? currentFilter,
        string? searchString,
        string? currentPublisher,
        string? publisher,
        int? pageNumber)
    {
        if (searchString != null || publisher != null)
        {
            pageNumber = 1;
        }
        else
        {
            searchString = currentFilter;
            publisher = currentPublisher;
        }

        ViewData["CurrentSort"] = sortOrder;
        ViewData["CurrentFilter"] = searchString;
        ViewData["CurrentPublisher"] = publisher;

        // V2 (B0): sortarea implicita este an descrescator
        ViewData["YearSortParm"] = string.IsNullOrEmpty(sortOrder) ? "year_asc" : "";
        ViewData["TitleSortParm"] = sortOrder == "title_asc" ? "title_desc" : "title_asc";
        ViewData["PriceSortParm"] = sortOrder == "price_asc" ? "price_desc" : "price_asc";
        ViewData["AuthorSortParm"] = sortOrder == "author_asc" ? "author_desc" : "author_asc";

        var publisherNames = await _context.Publisher
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => p.Name)
            .ToListAsync();
        ViewBag.Publishers = new SelectList(publisherNames, publisher);

        var books = _context.Book
            .Include(b => b.Author)
            .Include(b => b.Genre)
            .Include(b => b.Publishers)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            books = books.Where(b => b.Title.Contains(searchString));
        }

        // V3 (A1): filtru dupa editura (in SQL)
        if (!string.IsNullOrWhiteSpace(publisher))
        {
            books = books.Where(b => b.Publishers.Any(p => p.Name == publisher));
        }

        // Sortare in SQL dupa proprietatile mapate ale autorului (nu pe FullName nemapata)
        books = sortOrder switch
        {
            "year_asc" => books.OrderBy(b => b.Year).ThenBy(b => b.ID),
            "title_asc" => books.OrderBy(b => b.Title),
            "title_desc" => books.OrderByDescending(b => b.Title),
            "price_asc" => books.OrderBy(b => b.Price),
            "price_desc" => books.OrderByDescending(b => b.Price),
            "author_asc" => books.OrderBy(b => b.Author!.LastName).ThenBy(b => b.Author!.FirstName),
            "author_desc" => books.OrderByDescending(b => b.Author!.LastName).ThenByDescending(b => b.Author!.FirstName),
            _ => books.OrderByDescending(b => b.Year).ThenBy(b => b.ID) // V2 (B0): an descrescator
        };

        var rowsQuery = books.Select(b => new BookListItem(
            b.ID,
            b.Title,
            b.Author != null ? b.Author.FirstName + " " + b.Author.LastName : "",
            b.Genre != null ? b.Genre.Name : "",
            string.Join(", ", b.Publishers.Select(p => p.Name)),
            b.Year,
            b.Price,
            b.Orders.Count
        ));

        var paginatedList = await PaginatedList<BookListItem>.CreateAsync(
            rowsQuery, pageNumber ?? 1, PageSize);

        return View(paginatedList);
    }

    // GET: Books/Details/5 (A5: autor, gen, comenzi + cititor)
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var book = await _context.Book
            .Include(b => b.Author)
            .Include(b => b.Genre)
            .Include(b => b.Publishers)
            .Include(b => b.Orders)
                .ThenInclude(o => o.Customer)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ID == id);

        if (book == null) return NotFound();

        return View(book);
    }

    // GET: Books/Create
    public IActionResult Create()
    {
        ViewData["AuthorID"] = new SelectList(_context.Author.OrderBy(a => a.LastName), "ID", "FullName");
        ViewData["GenreID"] = new SelectList(_context.Genre.OrderBy(g => g.Name), "ID", "Name");
        return View();
    }

    // POST: Books/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ID,Title,Year,Price,Description,AuthorID,GenreID")] Book book)
    {
        if (ModelState.IsValid)
        {
            _context.Add(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["AuthorID"] = new SelectList(_context.Author.OrderBy(a => a.LastName), "ID", "FullName", book.AuthorID);
        ViewData["GenreID"] = new SelectList(_context.Genre.OrderBy(g => g.Name), "ID", "Name", book.GenreID);
        return View(book);
    }

    // GET: Books/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var book = await _context.Book.FindAsync(id);
        if (book == null) return NotFound();

        ViewData["AuthorID"] = new SelectList(_context.Author.OrderBy(a => a.LastName), "ID", "FullName", book.AuthorID);
        ViewData["GenreID"] = new SelectList(_context.Genre.OrderBy(g => g.Name), "ID", "Name", book.GenreID);
        return View(book);
    }

    // POST: Books/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ID,Title,Year,Price,AuthorID,GenreID")] Book bookInput)
    {
        if (id != bookInput.ID) return NotFound();

        var bookToUpdate = await _context.Book.FirstOrDefaultAsync(b => b.ID == id);
        if (bookToUpdate == null) return NotFound();

        if (ModelState.IsValid)
        {
            bookToUpdate.Title = bookInput.Title;
            bookToUpdate.Year = bookInput.Year;
            bookToUpdate.Price = bookInput.Price;
            bookToUpdate.AuthorID = bookInput.AuthorID;
            bookToUpdate.GenreID = bookInput.GenreID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(bookInput.ID)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewData["AuthorID"] = new SelectList(_context.Author.OrderBy(a => a.LastName), "ID", "FullName", bookInput.AuthorID);
        ViewData["GenreID"] = new SelectList(_context.Genre.OrderBy(g => g.Name), "ID", "Name", bookInput.GenreID);
        return View(bookToUpdate);
    }

    // GET: Books/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var book = await _context.Book
            .Include(b => b.Author)
            .Include(b => b.Genre)
            .FirstOrDefaultAsync(m => m.ID == id);
        if (book == null) return NotFound();

        return View(book);
    }

    // POST: Books/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var book = await _context.Book.FindAsync(id);
        if (book != null)
        {
            _context.Book.Remove(book);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool BookExists(int id)
    {
        return _context.Book.Any(e => e.ID == id);
    }
}
