# Laboratorul Î4 · Rad Sergiu Bogdan · nr. matricol 5146 · varianta A1 · B0

|Parametru|Valoarea mea|
|-|-|
|A (ultima cifră a nr. matricol: 6)|A1|
|B (penultima cifră a nr. matricol: 4)|B0|
|V1 · cărți pe pagină|6 cărți / pagină|
|V2 · sortarea implicită a catalogului|an desc.|
|V3 · filtrul din S3|editură|
|V4 · indicatorul din S2|cea mai comandată carte a autorului|

## S1 · Comenzile

* Protecția la overposting aleasă: **Bind**
* De ce am ales-o: Atributul `[Bind("CustomerID,BookID,OrderDate")]` pe acțiunile `Create` și `Edit` permite model binding doar pentru câmpurile permise explicit, împiedicând modificarea altor proprietăți trimise în formular/cerere.

## S2 · Pagina unui autor și sortarea după autor

* Indicatorul V4 (cea mai comandată carte a autorului) este calculat cu interogarea:

```sql
SELECT TOP(1) [b].[Title], (
    SELECT COUNT(*)
    FROM [Order] AS [o]
    WHERE [b].[ID] = [o].[BookID]) AS [OrdersCount]
FROM [Book] AS [b]
WHERE [b].[AuthorID] = @__id_0
ORDER BY (
    SELECT COUNT(*)
    FROM [Order] AS [o0]
    WHERE [b].[ID] = [o0].[BookID]) DESC
```

* De ce `OrderBy(b => b.Author.FullName)` aruncă o excepție? Proprietatea `FullName` este definită în C# ca proprietate calculată (`$"{FirstName} {LastName}"`) fără a fi mapată la o coloană în baza de date prin EF Core, astfel că provider-ul SQL nu o poate traduce într-o coloană SQL directă în clauza `ORDER BY`.
* Cum am rezolvat: Am sortat în interogarea LINQ / EF Core după proprietățile mapate ale autorului (`OrderBy(b => b.Author.LastName).ThenBy(b => b.Author.FirstName)`).

## S3 · Filtrul variantei mele (V3)

* Filtrul implementat: **Editură (`Publisher`)**
* Interogarea , cu filtrul aplicat:

```sql
SELECT [b].[ID], [b].[Title], [b].[AuthorID], [b].[GenreID], [b].[Price], [b].[Year], [a].[FirstName], [a].[LastName], [g].[Name]
FROM [Book] AS [b]
LEFT JOIN [Author] AS [a] ON [b].[AuthorID] = [a].[ID]
LEFT JOIN [Genre] AS [g] ON [b].[GenreID] = [g].[ID]
WHERE (@__searchString_0 = N'' OR CHARINDEX(@__searchString_0, [b].[Title]) > 0)
  AND (@__publisher_1 = N'' OR EXISTS (
      SELECT 1
      FROM [BookPublisher] AS [b0]
      INNER JOIN [Publisher] AS [p] ON [b0].[PublishersID] = [p].[ID]
      WHERE [b].[ID] = [b0].[BooksID] AND [p].[Name] = @__publisher_1))
ORDER BY [b].[Year] DESC, [b].[ID] ASC
OFFSET @__p_2 ROWS FETCH NEXT @__p_3 ROWS ONLY
```

* Scenariu testat (cu cel puțin 2 pagini de rezultate): Am filtrat catalogul după editură (fără filtru de titlu restrictiv, cu sortarea implicită an descrescător), verificând navigarea între pagini (`Înapoi` / `Înainte`) cu păstrarea parametrilor `sortOrder`, `currentFilter` și `currentPublisher`.

## S4 · Folosirea AI

* **Instrumente folosite:** Asistent AI (Gemini)
* **La ce pași:** Structurare entități și `LibraryContext`, implementare catalog (`A3–A5`), comenzi (`S1`), pagina autorului și sortare după autor (`S2`), filtrare după editură cu paginare (`S3`) și redactarea jurnalului.
* **Un exemplu de cerere trimisă:**

> da-mi ba toate fisierele intr-un zip sa descarc o data

* **O situație în care am corectat sau respins un răspuns AI** (sau, dacă nu am folosit AI, o eroare întâlnită și cum am găsit-o):

  * Ce a propus AI-ul / care era eroarea: Inițial, în unele acțiuni / interogări erau folosite denumiri de chei precum `Id` sau `CustomerId` acolo unde modelul din Întâlnirea 2 definește `ID` (`Author`, `Genre`, `Book`) și `CustomerID` (`Customer`), plus că varianta inițială din `BooksController` nu folosea proiecția `BookListItem` și `PaginatedList<T>` conform pașilor A3–A4.
  * Cum mi-am dat seama că e greșit sau ineficient: Comparând cu modelele din `Fisa2_Intalnirea2` și cerințele din `Laborator_I4` (proiecție `BookListItem`, `PaginatedList<T>`, `OrdersController`, `Authors/Details` cu V4 și `Books/Index` cu V1–V3).
  * Codul inițial:

```csharp
var customer = await _context.Customer.FirstOrDefaultAsync(m => m.Id == id);
```

  * Codul final:

```csharp
var customer = await _context.Customer.FirstOrDefaultAsync(m => m.CustomerID == id);
```

---

## Ce nu am reușit (opțional)

Interogările SQL din secțiunile S2 și S3 pot fi înlocuite cu logurile exacte afișate în fereastra Output / consolă la rularea proiectului local.
