
using Morning.Value.Application.Books.Dtos;
using Morning.Value.Application.Common.Dtos;

namespace Morning.Value.Application.Books.Services
{
    public interface IBookAppService
    {
        Task<Guid> CreateAsync(string title, string author, string genre, int availableCopies);
        Task<IEnumerable<BookResponse>> GetAllAsync();
        Task<PagedResult<BookResponse>> SearchAsync(string? q, int page, int pageSize);
    }
}
