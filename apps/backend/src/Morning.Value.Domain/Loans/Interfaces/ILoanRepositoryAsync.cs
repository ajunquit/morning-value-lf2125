using Morning.Value.Domain.Common.Interfaces;
using Morning.Value.Domain.Loans.Entity;

namespace Morning.Value.Domain.Loans.Interfaces
{
    public interface ILoanRepositoryAsync: IRepositoryAsync<Loan>
    {
        Task<Loan?> GetActiveByUserAndBookAsync(Guid userId, Guid bookId, CancellationToken ct = default);
    }
}
