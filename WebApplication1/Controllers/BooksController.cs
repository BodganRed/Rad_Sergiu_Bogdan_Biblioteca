using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class BooksController : Controller
{
    private readonly LibraryContext _context;

    public BooksController(LibraryContext context)
    {
        _context = context;
    }

    // GET: Books
    public async Task<IActionResult> Index()
    {
        var libraryContext = _context.Book.Include(b => b.Author).Include(b => b.Genre);
        return View(await libraryContext.ToListAsync());
    }

    // GET: Books/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var book = await _context.Book
            .Include(b => b.Author)
            .Include(b => b.Genre)
            .FirstOrDefaultAsync(m => m.ID == id);
        if (book == null) return NotFound();

        return View(book);
    }

    // GET: Books/Create
    public IActionResult Create()
    {
        ViewData["AuthorID"] = new SelectList(_context.Author, "ID", "LastName");
        ViewData["GenreID"] = new SelectList(_context.Genre, "ID", "Name");
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

        ViewData["AuthorID"] = new SelectList(_context.Author, "ID", "LastName", book.AuthorID);
        ViewData["GenreID"] = new SelectList(_context.Genre, "ID", "Name", book.GenreID);
        return View(book);
    }

    // GET: Books/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var book = await _context.Book.FindAsync(id);
        if (book == null) return NotFound();

        ViewData["AuthorID"] = new SelectList(_context.Author, "ID", "LastName", book.AuthorID);
        ViewData["GenreID"] = new SelectList(_context.Genre, "ID", "Name", book.GenreID);
        return View(book);
    }

    // VARIANTA VULNERABILĂ: doar pentru experiment
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Book book)
    {
        if (id != book.ID) return NotFound();
        _context.Update(book); // scrie TOATE coloanele
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
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
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool BookExists(int id)
    {
        return _context.Book.Any(e => e.ID == id);
    }
}