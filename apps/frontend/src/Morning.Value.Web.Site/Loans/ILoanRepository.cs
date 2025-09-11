using Morning.Value.Application.Common.Dtos;
using Morning.Value.Application.Loans.Dtos;
using Morning.Value.Domain.Loans.Enums;

namespace Morning.Value.Web.Site.Loans
{
    public interface ILoanRepository
    {
        Task<PagedResult<LoanHistoryItemResponse>> GetHistoryByUserAsync(
            string userId,
            string? query,
            LoanStatus? status,
            int pageIndex,
            int pageSize);
    }
}
