using Morning.Value.Domain.Common.Entities;
using Morning.Value.Domain.Exceptions;
using Morning.Value.Domain.Loans.Enums;

namespace Morning.Value.Domain.Loans.Entity
{
    public class Loan: AuditableEntity
    {
        public int UserId { get; private set; }
        public int BookId { get; private set; }

        public DateTime LoanDateUtc { get; private set; }
        public DateTime? ReturnDateUtc { get; private set; }

        public LoanStatus Status => ReturnDateUtc.HasValue ? LoanStatus.Returned : LoanStatus.Borrowed;

        private Loan() { } // EF

        public static Loan Create(int userId, int bookId, DateTime? whenUtc = null)
        {
            if (userId <= 0) throw new DomainException("UserId inválido.");
            if (bookId <= 0) throw new DomainException("BookId inválido.");

            return new Loan
            {
                UserId = userId,
                BookId = bookId,
                LoanDateUtc = whenUtc ?? DateTime.UtcNow
            };
        }

        public void MarkReturned(DateTime? whenUtc = null)
        {
            if (ReturnDateUtc.HasValue) throw new DomainException("El préstamo ya fue devuelto.");
            ReturnDateUtc = whenUtc ?? DateTime.UtcNow;
        }
    }
}
