using Morning.Value.Domain.Book.Interface;

namespace Morning.Value.Domain.Common.Interfaces
{
    public interface IUnitOfWorkAsync
    {
        IBookRepositoryAsync BookRepository { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
