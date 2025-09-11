using Morning.Value.Web.Site.Home.Models;
using static Morning.Value.Web.Site.Books.BookRepository;

namespace Morning.Value.Web.Site.Books
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllAsync();
        Task CreateAsync(string title, string author, string genre, int availableCopies);
    }
}
