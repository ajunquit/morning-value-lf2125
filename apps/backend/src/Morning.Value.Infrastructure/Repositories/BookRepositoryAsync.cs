using Microsoft.EntityFrameworkCore;
using Morning.Value.Domain.Book.Interfaces;
using Morning.Value.Domain.Books.Entity;
using Morning.Value.Infrastructure.Persistences.Contexts;

namespace Morning.Value.Infrastructure.Repositories
{
    public class BookRepositoryAsync(AppDbContext dbContext) : RepositoryAsync<Book>(dbContext), IBookRepositoryAsync
    {
        public async Task<(IReadOnlyList<Book> Items, int Total)> SearchAsync(string? q, int page, int pageSize)
        {
            var query = dbContext.Books.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var s = q.Trim().ToLower();
                query = query.Where(b =>
                    EF.Functions.Like(b.Title.ToLower(), $"%{s}%") ||
                    EF.Functions.Like(b.Author.ToLower(), $"%{s}%") ||
                    EF.Functions.Like(b.Genre.ToLower(), $"%{s}%"));
            }

            var total = await query.CountAsync();
            var items = await query.OrderBy(b => b.Title)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            return (items, total);
        }
    }
}
