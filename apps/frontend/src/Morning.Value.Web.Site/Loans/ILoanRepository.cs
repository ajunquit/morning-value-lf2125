using Morning.Value.Application.Common.Dtos;
using Morning.Value.Web.Site.Loans.Enums;
using Morning.Value.Web.Site.Loans.Models;

namespace Morning.Value.Web.Site.Loans
{
    public interface ILoanRepository
    {
        Task<PagedResult<LoanHistoryItem>> GetHistoryByUserAsync(
            string userId,
            string? query,
            LoanStatus? status,
            int pageIndex,
            int pageSize);
    }
}
