namespace WebApplication1.Models;

public record BookListItem(
    int ID,
    string Title,
    string Author,
    string Genre,
    string Publishers,
    int Year,
    decimal Price,
    int Orders
);
