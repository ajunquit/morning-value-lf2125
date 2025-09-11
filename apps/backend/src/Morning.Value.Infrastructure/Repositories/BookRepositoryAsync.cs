using Morning.Value.Domain.Book.Interface;
using Morning.Value.Domain.Books.Entity;
using Morning.Value.Infrastructure.Persistences.Contexts;

namespace Morning.Value.Infrastructure.Repositories
{
    public class BookRepositoryAsync(AppDbContext dbContext) : RepositoryAsync<Book>(dbContext), IBookRepositoryAsync
    {
    }
}
