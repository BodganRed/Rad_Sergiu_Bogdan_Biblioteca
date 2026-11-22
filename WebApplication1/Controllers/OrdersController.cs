using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class OrdersController : Controller
{
    private readonly LibraryContext _context;

    public OrdersController(LibraryContext context)
    {
        _context = context;
    }

    // GET: Orders
    public async Task<IActionResult> Index(int? customerId)
    {
        ViewData["CustomerList"] = new SelectList(
            await _context.Customer.OrderBy(c => c.Name).ToListAsync(),
            "CustomerID",
            "Name",
            customerId
        );
        ViewData["SelectedCustomerId"] = customerId;

        var ordersQuery = _context.Order
            .Include(o => o.Customer)
            .Include(o => o.Book)
            .AsNoTracking()
            .AsQueryable();

        if (customerId.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.CustomerID == customerId.Value);
        }

        var orders = await ordersQuery
            .OrderByDescending(o => o.OrderDate)
            .ThenByDescending(o => o.OrderID)
            .ToListAsync();

        return View(orders);
    }

    // GET: Orders/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var order = await _context.Order
            .Include(o => o.Customer)
            .Include(o => o.Book)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.OrderID == id);

        if (order == null) return NotFound();

        return View(order);
    }

    // GET: Orders/Create
    public IActionResult Create()
    {
        ViewData["CustomerID"] = new SelectList(_context.Customer.OrderBy(c => c.Name), "CustomerID", "Name");
        ViewData["BookID"] = new SelectList(_context.Book.OrderBy(b => b.Title), "ID", "Title");
        return View(new Order { OrderDate = DateTime.Today });
    }

    // POST: Orders/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CustomerID,BookID,OrderDate")] Order order)
    {
        if (ModelState.IsValid)
        {
            _context.Add(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["CustomerID"] = new SelectList(_context.Customer.OrderBy(c => c.Name), "CustomerID", "Name", order.CustomerID);
        ViewData["BookID"] = new SelectList(_context.Book.OrderBy(b => b.Title), "ID", "Title", order.BookID);
        return View(order);
    }

    // GET: Orders/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var order = await _context.Order.FindAsync(id);
        if (order == null) return NotFound();

        ViewData["CustomerID"] = new SelectList(_context.Customer.OrderBy(c => c.Name), "CustomerID", "Name", order.CustomerID);
        ViewData["BookID"] = new SelectList(_context.Book.OrderBy(b => b.Title), "ID", "Title", order.BookID);
        return View(order);
    }

    // POST: Orders/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("OrderID,CustomerID,BookID,OrderDate")] Order order)
    {
        if (id != order.OrderID) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(order);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(order.OrderID)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewData["CustomerID"] = new SelectList(_context.Customer.OrderBy(c => c.Name), "CustomerID", "Name", order.CustomerID);
        ViewData["BookID"] = new SelectList(_context.Book.OrderBy(b => b.Title), "ID", "Title", order.BookID);
        return View(order);
    }

    // GET: Orders/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var order = await _context.Order
            .Include(o => o.Customer)
            .Include(o => o.Book)
            .FirstOrDefaultAsync(m => m.OrderID == id);
        if (order == null) return NotFound();

        return View(order);
    }

    // POST: Orders/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var order = await _context.Order.FindAsync(id);
        if (order != null)
        {
            _context.Order.Remove(order);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool OrderExists(int id)
    {
        return _context.Order.Any(e => e.OrderID == id);
    }
}
