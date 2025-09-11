using Microsoft.EntityFrameworkCore;
using Morning.Value.Domain.Loans.Entity;
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
    }
}
