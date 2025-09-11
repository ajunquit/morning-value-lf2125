using Morning.Value.Domain.Common.Interfaces;

namespace Morning.Value.Domain.Book.Interface
{
    public interface IBookRepositoryAsync: IRepositoryAsync<Books.Entity.Book>
    {
        Task<(IReadOnlyList<Books.Entity.Book> Items, int Total)> SearchAsync(string? q, int page, int pageSize);
    }
}
