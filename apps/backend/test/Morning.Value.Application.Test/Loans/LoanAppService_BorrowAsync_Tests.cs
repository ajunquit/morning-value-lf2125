using FluentAssertions;
using Moq;
using Morning.Value.Application.Loans.Dtos;
using Morning.Value.Application.Loans.Services;
using Morning.Value.Domain.Book.Interfaces;
using Morning.Value.Domain.Books.Entity;
using Morning.Value.Domain.Common.Interfaces;
using Morning.Value.Domain.Exceptions;
using Morning.Value.Domain.Loans.Entity;
using Morning.Value.Domain.Loans.Interfaces;

namespace Morning.Value.Application.Test.Loans
{
    public class LoanAppService_BorrowAsync_Tests
    {
        private readonly Mock<IUnitOfWorkAsync> _uow = new();
        private readonly Mock<IBookRepositoryAsync> _bookRepo = new();
        private readonly Mock<ILoanRepositoryAsync> _loanRepo = new();

        private readonly LoanAppService _sut;

        public LoanAppService_BorrowAsync_Tests()
        {
            _uow.SetupGet(x => x.BookRepository).Returns(_bookRepo.Object);
            _uow.SetupGet(x => x.LoanRepository).Returns(_loanRepo.Object);

            _sut = new LoanAppService(_uow.Object);
        }

        [Fact]
        public async Task Borrow_HappyPath_CreatesLoan_DecrementsStock_AndCommits()
        {
            // arrange
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var book = new Book("Clean Architecture", "Robert C. Martin", "Tech", availableCopies: 3);

            _bookRepo.Setup(r => r.GetAsync(bookId)).ReturnsAsync(book);
            _loanRepo.Setup(r => r.GetActiveByUserAndBookAsync(userId, bookId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Loan?)null);

            _bookRepo.Setup(r => r.UpdateAsync(It.IsAny<Book>())).ReturnsAsync(true);
            _loanRepo.Setup(r => r.InsertAsync(It.IsAny<Loan>())).ReturnsAsync(true);
            _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // act
            BorrowResponse resp = await _sut.BorrowAsync(userId, bookId);

            // assert
            resp.BookId.Should().Be(bookId);
            resp.RemainingCopies.Should().Be(2); // 3 -> 2

            _bookRepo.Verify(r => r.GetAsync(bookId), Times.Once);
            _loanRepo.Verify(r => r.GetActiveByUserAndBookAsync(userId, bookId, It.IsAny<CancellationToken>()), Times.Once);
            _bookRepo.Verify(r => r.UpdateAsync(It.Is <Book>(b => b == book && b.AvailableCopies == 2)), Times.Once);
            _loanRepo.Verify(r => r.InsertAsync(It.Is<Loan>(l => l.BookId == bookId && l.UserId == userId)), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Borrow_WhenBookNotFound_ThrowsDomainException_AndDoesNotCommit()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();

            _bookRepo.Setup(r => r.GetAsync(bookId)).ReturnsAsync((Book?)null);

            Func<Task> act = async () => await _sut.BorrowAsync(userId, bookId);

            await act.Should().ThrowAsync<DomainException>()
                     .WithMessage("*Libro no encontrado*");

            _loanRepo.Verify(r => r.GetActiveByUserAndBookAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _bookRepo.Verify(r => r.UpdateAsync(It.IsAny<Book>()), Times.Never);
            _loanRepo.Verify(r => r.InsertAsync(It.IsAny<Loan>()), Times.Never);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Borrow_WithoutStock_ThrowsDomainException_AndDoesNotCreateLoan()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var book = new Book("DDD", "Evans", "Tech", availableCopies: 0);

            _bookRepo.Setup(r => r.GetAsync(bookId)).ReturnsAsync(book);

            Func<Task> act = async () => await _sut.BorrowAsync(userId, bookId);

            await act.Should().ThrowAsync<DomainException>()
                     .WithMessage("*No hay copias disponibles*");

            _loanRepo.Verify(r => r.GetActiveByUserAndBookAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _bookRepo.Verify(r => r.UpdateAsync(It.IsAny<Book>()), Times.Never);
            _loanRepo.Verify(r => r.InsertAsync(It.IsAny<Loan>()), Times.Never);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Borrow_WhenActiveLoanExists_ThrowsDomainException_AndDoesNotCommit()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var book = new Book("Book", "Author", "Genre", availableCopies: 5);
            var active = Loan.Create(userId, bookId);

            _bookRepo.Setup(r => r.GetAsync(bookId)).ReturnsAsync(book);
            _loanRepo.Setup(r => r.GetActiveByUserAndBookAsync(userId, bookId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(active);

            Func<Task> act = async () => await _sut.BorrowAsync(userId, bookId);

            await act.Should().ThrowAsync<DomainException>()
                     .WithMessage("*préstamo activo*");

            _bookRepo.Verify(r => r.UpdateAsync(It.IsAny<Book>()), Times.Never);
            _loanRepo.Verify(r => r.InsertAsync(It.IsAny<Loan>()), Times.Never);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}