using Morning.Value.Domain.Book.Interface;
using Morning.Value.Domain.Common.Interfaces;
using Morning.Value.Infrastructure.Persistences.Contexts;

namespace Morning.Value.Infrastructure.Repositories
{
    public class UnitOfWorkAsync(
        IBookRepositoryAsync bookRepository,
        AppDbContext appDbContext) : IUnitOfWorkAsync, IDisposable
    {
        private readonly AppDbContext _appDbContext = appDbContext;

        public IBookRepositoryAsync BookRepository { get; } = bookRepository;

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
