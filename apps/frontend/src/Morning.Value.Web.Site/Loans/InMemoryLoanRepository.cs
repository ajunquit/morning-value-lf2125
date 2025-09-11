using Morning.Value.Web.Site.Common.Models;
using Morning.Value.Web.Site.Loans.Enums;
using Morning.Value.Web.Site.Loans.Models;

namespace Morning.Value.Web.Site.Loans
{
    public class InMemoryLoanRepository : ILoanRepository
    {
        private readonly List<LoanHistoryItem> _data = new()
    {
        new() { LoanId=1, BookTitle="Clean Architecture", Author="Robert C. Martin", Genre="Tech", LoanDate=DateTime.UtcNow.AddDays(-10), ReturnDate=DateTime.UtcNow.AddDays(-5) },
        new() { LoanId=2, BookTitle="Domain-Driven Design", Author="Eric Evans", Genre="Tech", LoanDate=DateTime.UtcNow.AddDays(-3), ReturnDate=null },
        // agrega más…
    };

        public Task<PagedResult<LoanHistoryItem>> GetHistoryByUserAsync(
            string userId, string? query, LoanStatus? status, int pageIndex, int pageSize)
        {
            IEnumerable<LoanHistoryItem> q = _data; // en real: filtra por userId
            if (!string.IsNullOrWhiteSpace(query))
            {
                var s = query.Trim().ToLower();
                q = q.Where(x =>
                    x.BookTitle.ToLower().Contains(s) ||
                    x.Author.ToLower().Contains(s) ||
                    x.Genre.ToLower().Contains(s));
            }
            if (status.HasValue)
            {
                q = q.Where(x => (x.ReturnDate.HasValue) == (status == LoanStatus.Returned));
            }

            var total = q.Count();
            var items = q
                .OrderByDescending(x => x.LoanDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new PagedResult<LoanHistoryItem>
            {
                Items = items,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = total,
                Query = query,
                StatusFilter = status?.ToString().ToLower() ?? "all"
            };
            return Task.FromResult(result);
        }
    }

}
