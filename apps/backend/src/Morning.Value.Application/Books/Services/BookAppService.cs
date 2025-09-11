using Morning.Value.Domain.Common.Interfaces;

namespace Morning.Value.Application.Books.Services
{
    public class BookAppService(IUnitOfWorkAsync unitOfWorkAsync) : IBookAppService
    {
        private readonly IUnitOfWorkAsync _uow = unitOfWorkAsync;
    }
}
