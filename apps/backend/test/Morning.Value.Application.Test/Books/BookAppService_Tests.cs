using FluentAssertions;
using Moq;
using Morning.Value.Application.Books.Services;
using Morning.Value.Application.Common.Dtos;
using Morning.Value.Domain.Book.Interfaces;
using Morning.Value.Domain.Books.Entity;
using Morning.Value.Domain.Common.Interfaces;
using Morning.Value.Domain.Exceptions;

namespace Morning.Value.Application.Test.Books
{
    public class BookAppService_Tests
    {
        private readonly Mock<IUnitOfWorkAsync> _uow = new();
        private readonly Mock<IBookRepositoryAsync> _bookRepo = new();

        private readonly BookAppService _sut;

        public BookAppService_Tests()
        {
            _uow.SetupGet(x => x.BookRepository).Returns(_bookRepo.Object);
            _sut = new BookAppService(_uow.Object);
        }

        // ----------------- CreateAsync -----------------

        [Fact]
        public async Task CreateAsync_Valid_InsertsAndSaves_ReturnsNonEmptyId()
        {
            // arrange
            _bookRepo.Setup(r => r.InsertAsync(It.IsAny<Book>())).ReturnsAsync(true);
            _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // act
            var id = await _sut.CreateAsync("Clean Architecture", "Robert C. Martin", "Tech", 5);

            // assert
            id.Should().NotBe(Guid.Empty);

            _bookRepo.Verify(r => r.InsertAsync(It.Is<Book>(b =>
                b.Title == "Clean Architecture" &&
                b.Author == "Robert C. Martin" &&
                b.Genre == "Tech" &&
                b.AvailableCopies == 5
            )), Times.Once);

            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData("", "Author", "Genre", 1)]
        [InlineData("Title", "", "Genre", 1)]
        [InlineData("Title", "Author", "", 1)]
        public async Task CreateAsync_InvalidData_ThrowsDomainException_AndDoesNotInsert(
            string title, string author, string genre, int copies)
        {
            // act
            Func<Task> act = async () => await _sut.CreateAsync(title, author, genre, copies);

            // assert
            await act.Should().ThrowAsync<DomainException>();

            _bookRepo.Verify(r => r.InsertAsync(It.IsAny<Book>()), Times.Never);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        // ----------------- GetAllAsync -----------------

        [Fact]
        public async Task GetAllAsync_MapsEntitiesToResponse()
        {
            var books = new List<Book>
            {
                new Book("Clean Architecture", "Robert C. Martin", "Tech", 3),
                new Book("DDD", "Eric Evans", "Tech", 2)
            }.AsEnumerable();

            _bookRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(books);

            var result = await _sut.GetAllAsync();

            result.Should().HaveCount(2);
            result.Should().Contain(x => x.Title == "Clean Architecture" && x.Author == "Robert C. Martin");
            result.Should().Contain(x => x.Title == "DDD" && x.Author == "Eric Evans");

            _bookRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        // ----------------- SearchAsync -----------------
        // Nota: IBookRepositoryAsync.SearchAsync devuelve (Items, Total)

        [Fact]
        public async Task SearchAsync_WithoutQuery_UsesRepoResult_OrdersAndPaginates()
        {
            // arrange: el repo devuelve 3 libros
            var repoItems = new List<Book>
            {
                new Book("C Book", "Author", "Tech", 1),
                new Book("A Book", "Author", "Tech", 2),
                new Book("B Book", "Author", "Tech", 3)
            };

            var tuple = (Items: (IReadOnlyList<Book>)repoItems.AsReadOnly(), Total: repoItems.Count);

            _bookRepo
                .Setup(r => r.SearchAsync(null, 1, 2))
                .ReturnsAsync(tuple);

            // act
            var grid = await _sut.SearchAsync(null, page: 1, pageSize: 2);

            // assert: el servicio ordena por Title asc y pagina
            grid.PageIndex.Should().Be(1);
            grid.PageSize.Should().Be(2);
            grid.TotalCount.Should().Be(3); // el servicio recalcula total = query.Count()

            grid.Items.Should().HaveCount(2);
            grid.Items.Select(x => x.Title).Should().ContainInOrder("A Book", "B Book");

            _bookRepo.Verify(r => r.SearchAsync(null, 1, 2), Times.Once);
        }

        [Fact]
        public async Task SearchAsync_WithQuery_Filters_Orders_AndPaginates()
        {
            var repoItems = new List<Book>
            {
                new Book("Refactoring", "Fowler", "Tech", 1),
                new Book("The Pragmatic Programmer", "Hunt", "Tech", 1),
                new Book("Clean Architecture", "Robert C. Martin", "Tech", 1),
                new Book("Clean Code", "Robert C. Martin", "Tech", 1)
            };

            // Devolvemos todos; el servicio vuelve a filtrar/ordenar/paginar.
            var tuple = (Items: (IReadOnlyList<Book>)repoItems.AsReadOnly(), Total: repoItems.Count);

            _bookRepo
                .Setup(r => r.SearchAsync("clean", It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(tuple);

            // act: buscamos "clean" y pedimos page=1, size=2
            var grid = await _sut.SearchAsync("clean", page: 1, pageSize: 2);

            // assert: quedan "Clean Architecture" y "Clean Code"
            grid.TotalCount.Should().Be(2);
            grid.Items.Should().HaveCount(2);
            grid.Items.Select(x => x.Title).Should().ContainInOrder("Clean Architecture", "Clean Code");

            _bookRepo.Verify(r => r.SearchAsync("clean", 1, 2), Times.Once);
        }
    }
}
