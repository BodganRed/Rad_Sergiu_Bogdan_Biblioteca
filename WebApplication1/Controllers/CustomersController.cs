using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class CustomersController : Controller
{
    private readonly LibraryContext _context;

    public CustomersController(LibraryContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Customer.OrderBy(c => c.Name).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var customer = await _context.Customer
            .FirstOrDefaultAsync(m => m.CustomerID == id);
        if (customer == null) return NotFound();

        return View(customer);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CustomerID,Name,Adress,BirthDate")] Customer customer)
    {
        if (ModelState.IsValid)
        {
            _context.Add(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(customer);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var customer = await _context.Customer.FindAsync(id);
        if (customer == null) return NotFound();
        return View(customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CustomerID,Name,Adress,BirthDate")] Customer customer)
    {
        if (id != customer.CustomerID) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(customer);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(customer.CustomerID)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(customer);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var customer = await _context.Customer
            .FirstOrDefaultAsync(m => m.CustomerID == id);
        if (customer == null) return NotFound();

        return View(customer);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var customer = await _context.Customer.FindAsync(id);
        if (customer != null)
        {
            _context.Customer.Remove(customer);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool CustomerExists(int id)
    {
        return _context.Customer.Any(e => e.CustomerID == id);
    }
}
