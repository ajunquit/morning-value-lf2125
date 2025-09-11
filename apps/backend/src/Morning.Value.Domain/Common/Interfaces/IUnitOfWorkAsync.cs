using Morning.Value.Domain.Book.Interfaces;
using Morning.Value.Domain.Loans.Interfaces;
using Morning.Value.Domain.Users.Interfaces;

namespace Morning.Value.Domain.Common.Interfaces
{
    public interface IUnitOfWorkAsync
    {
        IBookRepositoryAsync BookRepository { get; }
        ILoanRepositoryAsync LoanRepository { get; }
        IUserRepositoryAsync UserRepository { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
