namespace Morning.Value.Domain.Loans.Entity
{
    public class LoanHistoryRow
    {
        public Guid LoanId { get; init; }
        public string BookTitle { get; init; } = string.Empty;
        public string Author { get; init; } = string.Empty;
        public string Genre { get; init; } = string.Empty;
        public DateTime LoanDateUtc { get; init; }
        public DateTime? ReturnDateUtc { get; init; }
    }
}
