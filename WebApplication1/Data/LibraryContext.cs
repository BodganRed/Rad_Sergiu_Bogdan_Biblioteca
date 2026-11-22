using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data;

public class LibraryContext(DbContextOptions<LibraryContext> options) : DbContext(options)
{
    public DbSet<Book> Book { get; set; } = default!;
    public DbSet<Author> Author { get; set; } = default!;
    public DbSet<Genre> Genre { get; set; } = default!;
    public DbSet<Publisher> Publisher { get; set; } = default!;
    public DbSet<Customer> Customer { get; set; } = default!;
    public DbSet<Order> Order { get; set; } = default!;
}
