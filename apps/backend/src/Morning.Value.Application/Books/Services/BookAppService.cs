using Morning.Value.Application.Books.Dtos;
using Morning.Value.Application.Common.Dtos;
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
            return book.Id;
        }

        public async Task<IEnumerable<BookResponse>> GetAllAsync()
        {
            var list = _uow.BookRepository.GetAllAsync();
            var result = list.Result.Select(b => new BookResponse
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                Genre = b.Genre,
                AvailableCopies = b.AvailableCopies
            });

            return await Task.FromResult(result);
        }

        public async Task<PagedResult<BookResponse>> SearchAsync(string? q, int page, int pageSize)
        {
            var list = await _uow.BookRepository.SearchAsync(q, page, pageSize);
            var query = list.Items.Select(b => new BookResponse()
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                Genre = b.Genre,
                AvailableCopies = b.AvailableCopies
            });

            if (!string.IsNullOrWhiteSpace(q))
            {
                var s = q.Trim().ToLowerInvariant();
                query = query.Where(b =>
                    b.Title.ToLower().Contains(s) ||
                    b.Author.ToLower().Contains(s) ||
                    b.Genre.ToLower().Contains(s));
            }

            var total = query.Count();
            var items = query
                .OrderBy(b => b.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookResponse
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Genre = b.Genre,
                    AvailableCopies = b.AvailableCopies
                })
                .ToList();

            return await Task.FromResult(new PagedResult<BookResponse>
            {
                Items = items,
                PageIndex = page,
                PageSize = pageSize,
                TotalCount = total,
                Query = q
            });
        }
    }
}
