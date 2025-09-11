using Morning.Value.Domain.Books.Entity;
using Morning.Value.Domain.Common.Interfaces;

namespace Morning.Value.Application.Books.Services
{
    public class BookAppService(IUnitOfWorkAsync unitOfWorkAsync) : IBookAppService
    {
        private readonly IUnitOfWorkAsync _uow = unitOfWorkAsync;

        public async Task<Guid> CreateAsync(string title, string author, string genre, int availableCopies)
        {
            var book = new Book(title, author, genre, availableCopies);
            await _uow.BookRepository.InsertAsync(book);
            await _uow.SaveChangesAsync();
            return book.Id; // Guid
        }
    }
}
