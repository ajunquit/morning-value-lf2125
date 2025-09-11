using Morning.Value.Web.Site.Books.Models;
using Morning.Value.Web.Site.Common.Models;
using static Morning.Value.Web.Site.Books.BookRepository;

namespace Morning.Value.Web.Site.Books
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllAsync();
        Task CreateAsync(string title, string author, string genre, int availableCopies);
        // Nuevo: búsqueda con paginación por título/autor/género
        Task<PagedResult<BookListItem>> SearchAsync(string? q, int page, int pageSize);
        Task<Book?> GetByIdAsync(int id);
        Task UpdateAsync(Book book);
    }
}
