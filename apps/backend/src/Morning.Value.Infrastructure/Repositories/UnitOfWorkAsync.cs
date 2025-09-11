using Morning.Value.Domain.Book.Interfaces;
using Morning.Value.Domain.Common.Interfaces;
using Morning.Value.Domain.Loans.Interfaces;
using Morning.Value.Domain.Users.Interfaces;
using Morning.Value.Infrastructure.Persistences.Contexts;

namespace Morning.Value.Infrastructure.Repositories
{
    public class UnitOfWorkAsync(
        IUserRepositoryAsync userRepository,
        ILoanRepositoryAsync loanRepository,
        IBookRepositoryAsync bookRepository,
        AppDbContext appDbContext) : IUnitOfWorkAsync, IDisposable
    {
        private readonly AppDbContext _appDbContext = appDbContext;

        public IBookRepositoryAsync BookRepository { get; } = bookRepository;

        public ILoanRepositoryAsync LoanRepository { get; } = loanRepository;

        public IUserRepositoryAsync UserRepository { get; } = userRepository;

        public void Dispose()
        {
            _appDbContext?.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _appDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
