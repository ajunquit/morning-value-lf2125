using Morning.Value.Domain.Common.Entities;
using Morning.Value.Domain.Exceptions;
using Morning.Value.Domain.Loans.Enums;

namespace Morning.Value.Domain.Loans.Entity
{
    public class Loan: AuditableEntity
    {
        public Guid UserId { get; private set; }
        public Guid BookId { get; private set; }

        public DateTime LoanDateUtc { get; private set; }
        public DateTime? ReturnDateUtc { get; private set; }

        public LoanStatus Status => ReturnDateUtc.HasValue ? LoanStatus.Returned : LoanStatus.Borrowed;

        private Loan() { } 

        public static Loan Create(Guid userId, Guid bookId, DateTime? whenUtc = null)
        {
            if (userId == Guid.Empty) throw new DomainException("UserId inválido.");
            if (bookId == Guid.Empty) throw new DomainException("BookId inválido.");

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
