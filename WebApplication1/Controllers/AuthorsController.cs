using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class AuthorsController : Controller
{
    private readonly LibraryContext _context;

    public AuthorsController(LibraryContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Author.OrderBy(a => a.LastName).ThenBy(a => a.FirstName).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var author = await _context.Author
            .Include(a => a.Books)
                .ThenInclude(b => b.Genre)
            .Include(a => a.Books)
                .ThenInclude(b => b.Orders)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ID == id);

        if (author == null) return NotFound();

        int totalOrders = author.Books.Sum(b => b.Orders.Count);
        ViewData["TotalOrders"] = totalOrders;

        // V4 (B0): cea mai comandata carte a autorului, calculata in SQL
        var topBookSql = await _context.Book
            .AsNoTracking()
            .Where(b => b.AuthorID == id.Value)
            .OrderByDescending(b => b.Orders.Count)
            .Select(b => new { b.Title, OrdersCount = b.Orders.Count })
            .FirstOrDefaultAsync();

        ViewData["MostOrderedBook"] = topBookSql != null && topBookSql.OrdersCount > 0
            ? $"{topBookSql.Title} ({topBookSql.OrdersCount} comenzi)"
            : "Nicio carte comandată";

        return View(author);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ID,FirstName,LastName")] Author author)
    {
        if (ModelState.IsValid)
        {
            _context.Add(author);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(author);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var author = await _context.Author.FindAsync(id);
        if (author == null) return NotFound();
        return View(author);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ID,FirstName,LastName")] Author author)
    {
        if (id != author.ID) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(author);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AuthorExists(author.ID)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(author);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var author = await _context.Author.FirstOrDefaultAsync(m => m.ID == id);
        if (author == null) return NotFound();

        return View(author);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var author = await _context.Author.FindAsync(id);
        if (author != null)
        {
            _context.Author.Remove(author);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool AuthorExists(int id)
    {
        return _context.Author.Any(e => e.ID == id);
    }
}
