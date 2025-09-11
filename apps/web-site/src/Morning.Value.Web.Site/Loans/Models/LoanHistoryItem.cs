using Morning.Value.Web.Site.Loans.Enums;

namespace Morning.Value.Web.Site.Loans.Models
{
    public class LoanHistoryItem
    {
        public int LoanId { get; set; }
        public string BookTitle { get; set; } = "";
        public string Author { get; set; } = "";
        public string Genre { get; set; } = "";
        public DateTime LoanDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public LoanStatus Status => ReturnDate.HasValue ? LoanStatus.Returned : LoanStatus.Borrowed;
    }
}
