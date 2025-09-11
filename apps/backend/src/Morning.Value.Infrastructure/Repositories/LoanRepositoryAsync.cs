using Microsoft.EntityFrameworkCore;
using Morning.Value.Domain.Loans.Entity;
using Morning.Value.Domain.Loans.Enums;
using Morning.Value.Domain.Loans.Interfaces;
using Morning.Value.Infrastructure.Persistences.Contexts;

namespace Morning.Value.Infrastructure.Repositories
{
    public class LoanRepositoryAsync(AppDbContext dbContext) : RepositoryAsync<Loan>(dbContext), ILoanRepositoryAsync
    {
        public async Task<Loan?> GetActiveByUserAndBookAsync(Guid userId, Guid bookId, CancellationToken ct = default)
        {
            return await _entities
                .Where(l => l.UserId == userId && l.BookId == bookId && !l.ReturnDateUtc.HasValue)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<(IReadOnlyList<LoanHistoryRow> Items, int Total)> GetHistoryByUserAsync(
            Guid userId, string? query, LoanStatus? status, int page, int pageSize, CancellationToken ct = default)
        {
            var q = from l in dbContext.Loans.AsNoTracking()
                    join b in dbContext.Books.AsNoTracking() on l.BookId equals b.Id
                    where l.UserId == userId
                    select new { l.Id, b.Title, b.Author, b.Genre, l.LoanDateUtc, l.ReturnDateUtc };

            if (!string.IsNullOrWhiteSpace(query))
            {
                var s = query.Trim();
                var sLike = $"%{s}%";
                q = q.Where(x =>
                    EF.Functions.Like(x.Title, sLike) ||
                    EF.Functions.Like(x.Author, sLike) ||
                    EF.Functions.Like(x.Genre, sLike));
            }

            if (status == LoanStatus.Borrowed)
                q = q.Where(x => x.ReturnDateUtc == null);
            else if (status == LoanStatus.Returned)
                q = q.Where(x => x.ReturnDateUtc != null);

            var total = await q.CountAsync(ct);

            var rows = await q
                .OrderByDescending(x => x.LoanDateUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LoanHistoryRow
                {
                    LoanId = x.Id,
                    BookTitle = x.Title,
                    Author = x.Author,
                    Genre = x.Genre,
                    LoanDateUtc = x.LoanDateUtc,
                    ReturnDateUtc = x.ReturnDateUtc
                })
                .ToListAsync(ct);

            return (rows, total);
        }
    }
}
