using Morning.Value.Application.Common.Dtos;
using Morning.Value.Application.Loans.Dtos;
using Morning.Value.Domain.Loans.Enums;

namespace Morning.Value.Application.Loans.Services
{
    public interface ILoanAppService
    {
        Task<BorrowResponse> BorrowAsync(Guid userId, Guid bookId, CancellationToken ct = default);
        Task<bool> ReturnAsync(Guid loanId, CancellationToken ct = default);
        Task<PagedResult<LoanHistoryItemResponse>> GetHistoryByUserAsync(
            Guid userId, string? query, LoanStatus? status, int page, int pageSize,
            CancellationToken ct = default);
    }
}
