using Morning.Value.Application.Common.Dtos;
using Morning.Value.Web.Site.Books.Models;
using Morning.Value.Web.Site.Common.Models;

namespace Morning.Value.Web.Site.Books
{
    public class BookRepository : IBookRepository
    {
        // Semilla simple para pruebas (puedes moverla a un seeder)
        private static readonly List<Book> _books = new()
    {
        new Book { Id=1, Title="Clean Architecture", Author="Robert C. Martin", Genre="Tech", AvailableCopies=3 },
        new Book { Id=2, Title="Domain-Driven Design", Author="Eric Evans", Genre="Tech", AvailableCopies=0 },
        new Book { Id=3, Title="El Quijote", Author="Miguel de Cervantes", Genre="Ficción", AvailableCopies=1 },
        new Book { Id=4, Title="Refactoring", Author="Martin Fowler", Genre="Tech", AvailableCopies=5 },
    };

        public Task CreateAsync(string title, string author, string genre, int availableCopies)
        {
            var nextId = _books.Count == 0 ? 1 : _books.Max(b => b.Id) + 1;
            _books.Add(new Book
            {
                Id = nextId,
                Title = title,
                Author = author,
                Genre = genre,
                AvailableCopies = Math.Max(0, availableCopies)
            });

            return Task.CompletedTask;
        }

        public Task<IEnumerable<Book>> GetAllAsync()
        {
            var items = _books
                .OrderBy(b => b.Title)
                .Select(b => new Book
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Genre = b.Genre,
                    AvailableCopies = b.AvailableCopies // int (>=0)
                })
                .AsEnumerable();

            return Task.FromResult(items);
        }

        public Task<PagedResult<BookListItem>> SearchAsync(string? q, int page, int pageSize)
        {
            IEnumerable<Book> query = _books;

            if (!string.IsNullOrWhiteSpace(q))
            {
                var s = q.Trim().ToLowerInvariant();
                query = query.Where(b =>
                    b.Title.ToLower().Contains(s) ||
                    b.Author.ToLower().Contains(s) ||
                    b.Genre.ToLower().Contains(s));
            }

            var total = query.Count();
            var items = query
                .OrderBy(b => b.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookListItem
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Genre = b.Genre,
                    AvailableCopies = b.AvailableCopies
                })
                .ToList();

            return Task.FromResult(new PagedResult<BookListItem>
            {
                Items = items,
                PageIndex = page,
                PageSize = pageSize,
                TotalCount = total,
                Query = q
            });
        }

        public Task<Book?> GetByIdAsync(int id) => Task.FromResult(_books.FirstOrDefault(b => b.Id == id));

        public Task UpdateAsync(Book book)
        {
            var i = _books.FindIndex(b => b.Id == book.Id);
            if (i >= 0) _books[i] = book;
            return Task.CompletedTask;
        }

        public class Book
        {
            public int Id { get; set; }
            public string Title { get; set; } = "";
            public string Author { get; set; } = "";
            public string Genre { get; set; } = "";
            public int AvailableCopies { get; set; } // inventario
        }
    }
}
