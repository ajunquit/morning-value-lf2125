using Morning.Value.Domain.Loans.Enums;

namespace Morning.Value.Application.Loans.Dtos
{
    public class LoanHistoryItemResponse
    {
        public Guid LoanId { get; init; }
        public string BookTitle { get; init; } = string.Empty;
        public string Author { get; init; } = string.Empty;
        public string Genre { get; init; } = string.Empty;
        public DateTime LoanDate { get; init; }
        public DateTime? ReturnDate { get; init; }
        public LoanStatus Status { get; init; }
    }
}
