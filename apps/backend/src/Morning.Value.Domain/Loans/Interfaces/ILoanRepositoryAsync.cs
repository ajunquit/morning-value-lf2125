using Morning.Value.Domain.Common.Interfaces;
using Morning.Value.Domain.Loans.Entity;
using Morning.Value.Domain.Loans.Enums;

namespace Morning.Value.Domain.Loans.Interfaces
{
    public interface ILoanRepositoryAsync: IRepositoryAsync<Loan>
    {
        Task<Loan?> GetActiveByUserAndBookAsync(Guid userId, Guid bookId, CancellationToken ct = default);
        Task<(IReadOnlyList<LoanHistoryRow> Items, int Total)> GetHistoryByUserAsync(
            Guid userId, string? query, LoanStatus? status, int page, int pageSize,
            CancellationToken ct = default);
    }
}
