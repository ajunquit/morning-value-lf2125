using Morning.Value.Application.Loans.Dtos;

namespace Morning.Value.Application.Loans.Services
{
    public interface ILoanAppService
    {
        Task<BorrowResponse> BorrowAsync(Guid userId, Guid bookId, CancellationToken ct = default);
        Task<bool> ReturnAsync(Guid loanId, CancellationToken ct = default);
    }
}
