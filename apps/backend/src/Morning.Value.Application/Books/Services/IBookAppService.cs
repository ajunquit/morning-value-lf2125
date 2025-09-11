
namespace Morning.Value.Application.Books.Services
{
    public interface IBookAppService
    {
        Task<Guid> CreateAsync(string title, string author, string genre, int availableCopies);
    }
}
