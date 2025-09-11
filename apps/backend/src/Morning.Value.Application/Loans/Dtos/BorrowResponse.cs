namespace Morning.Value.Application.Loans.Dtos
{
    public class BorrowResponse
    {
        public Guid LoanId { get; init; }
        public Guid BookId { get; init; }
        public int RemainingCopies { get; init; }
        public DateTime LoanDateUtc { get; init; }
    }
}
