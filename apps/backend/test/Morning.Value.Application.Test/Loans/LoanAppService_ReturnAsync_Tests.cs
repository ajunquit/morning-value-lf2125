using FluentAssertions;
using Moq;
using Morning.Value.Application.Loans.Services;
using Morning.Value.Domain.Book.Interfaces;
using Morning.Value.Domain.Books.Entity;
using Morning.Value.Domain.Common.Interfaces;
using Morning.Value.Domain.Exceptions;
using Morning.Value.Domain.Loans.Entity;
using Morning.Value.Domain.Loans.Interfaces;

namespace Morning.Value.Application.Test.Loans
{
    public class LoanAppService_ReturnAsync_Tests
    {
        private readonly Mock<IUnitOfWorkAsync> _uow = new();
        private readonly Mock<IBookRepositoryAsync> _bookRepo = new();
        private readonly Mock<ILoanRepositoryAsync> _loanRepo = new();

        private readonly LoanAppService _sut;

        public LoanAppService_ReturnAsync_Tests()
        {
            _uow.SetupGet(x => x.BookRepository).Returns(_bookRepo.Object);
            _uow.SetupGet(x => x.LoanRepository).Returns(_loanRepo.Object);

            _sut = new LoanAppService(_uow.Object);
        }

        [Fact]
        public async Task Return_HappyPath_MarksReturned_IncrementsStock_AndCommits()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var loanId = Guid.NewGuid(); // no se usa para crear Loan, pero lo verificamos por ref
            var loan = Loan.Create(userId, bookId);
            // Simular que el repo retorna exactamente este Loan con ese Id
            typeof(Loan).GetProperty(nameof(Loan.Id))!.SetValue(loan, loanId);

            var book = new Book("DDD", "Evans", "Tech", availableCopies: 0);

            _loanRepo.Setup(r => r.GetAsync(loanId)).ReturnsAsync(loan);
            _bookRepo.Setup(r => r.GetAsync(bookId)).ReturnsAsync(book);

            _loanRepo.Setup(r => r.UpdateAsync(It.IsAny<Loan>())).ReturnsAsync(true);
            _bookRepo.Setup(r => r.UpdateAsync(It.IsAny<Book>())).ReturnsAsync(true);
            _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var ok = await _sut.ReturnAsync(loanId);

            ok.Should().BeTrue();
            loan.ReturnDateUtc.Should().NotBeNull();
            book.AvailableCopies.Should().Be(1);

            _loanRepo.Verify(r => r.GetAsync(loanId), Times.Once);
            _bookRepo.Verify(r => r.GetAsync(bookId), Times.Once);
            _loanRepo.Verify(r => r.UpdateAsync(It.Is<Loan>(l => l == loan && l.ReturnDateUtc != null)), Times.Once);
            _bookRepo.Verify(r => r.UpdateAsync(It.Is<Book>(b => b == book && b.AvailableCopies == 1)), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Return_WhenLoanNotFound_ReturnsFalse()
        {
            var loanId = Guid.NewGuid();
            _loanRepo.Setup(r => r.GetAsync(loanId)).ReturnsAsync((Loan?)null);

            var ok = await _sut.ReturnAsync(loanId);

            ok.Should().BeFalse();
            _bookRepo.Verify(r => r.GetAsync(It.IsAny<Guid>()), Times.Never);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Return_WhenAlreadyReturned_ReturnsFalse_AndDoesNotCommit()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var loan = Loan.Create(userId, bookId);
            loan.MarkReturned();

            _loanRepo.Setup(r => r.GetAsync(It.IsAny<Guid>())).ReturnsAsync(loan);

            var ok = await _sut.ReturnAsync(Guid.NewGuid());

            ok.Should().BeFalse();
            _bookRepo.Verify(r => r.GetAsync(It.IsAny<Guid>()), Times.Never);
            _loanRepo.Verify(r => r.UpdateAsync(It.IsAny<Loan>()), Times.Never);
            _bookRepo.Verify(r => r.UpdateAsync(It.IsAny<Book>()), Times.Never);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Return_WhenBookNotFound_ThrowsDomainException()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var loan = Loan.Create(userId, bookId);

            _loanRepo.Setup(r => r.GetAsync(It.IsAny<Guid>())).ReturnsAsync(loan);
            _bookRepo.Setup(r => r.GetAsync(bookId)).ReturnsAsync((Book?)null);

            Func<Task> act = async () => await _sut.ReturnAsync(Guid.NewGuid());

            await act.Should().ThrowAsync<DomainException>()
                     .WithMessage("*Libro del préstamo no encontrado*");

            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
