using FluentAssertions;
using Morning.Value.Domain.Exceptions;
using Morning.Value.Domain.Loans.Entity;
using Morning.Value.Domain.Loans.Enums;

namespace Morning.Value.Domain.Test
{
    public class LoadTests
    {
        public class LoanTests
        {
            [Fact]
            public void Create_Valid_SetsProps_AndStatusBorrowed()
            {
                // arrange
                var userId = Guid.NewGuid();
                var bookId = Guid.NewGuid();
                var t0 = DateTime.UtcNow;

                // act
                var loan = Loan.Create(userId, bookId);

                // assert
                loan.UserId.Should().Be(userId);
                loan.BookId.Should().Be(bookId);
                loan.Status.Should().Be(LoanStatus.Borrowed);
                loan.ReturnDateUtc.Should().BeNull();

                // LoanDateUtc se setea “ahora”
                loan.LoanDateUtc.Should().BeOnOrAfter(t0).And.BeOnOrBefore(DateTime.UtcNow);
            }

            [Fact]
            public void Create_WithExplicitWhenUtc_UsesProvidedTime()
            {
                var userId = Guid.NewGuid();
                var bookId = Guid.NewGuid();
                var when = new DateTime(2024, 12, 31, 23, 59, 59, DateTimeKind.Utc);

                var loan = Loan.Create(userId, bookId, whenUtc: when);

                loan.LoanDateUtc.Should().Be(when);
            }

            [Theory]
            [InlineData(true, false)]   // userId vacío
            [InlineData(false, true)]   // bookId vacío
            public void Create_InvalidIds_ThrowsDomainException(bool emptyUser, bool emptyBook)
            {
                var userId = emptyUser ? Guid.Empty : Guid.NewGuid();
                var bookId = emptyBook ? Guid.Empty : Guid.NewGuid();

                Action act = () => Loan.Create(userId, bookId);

                act.Should().Throw<DomainException>()
                   .WithMessage("*inválido*");
            }

            [Fact]
            public void MarkReturned_SetsReturnDate_AndStatusReturned()
            {
                // arrange
                var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid(),
                                       whenUtc: new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc));
                var ret = new DateTime(2025, 01, 10, 12, 30, 00, DateTimeKind.Utc);

                // act
                loan.MarkReturned(ret);

                // assert
                loan.ReturnDateUtc.Should().Be(ret);
                loan.Status.Should().Be(LoanStatus.Returned);
            }

            [Fact]
            public void MarkReturned_Twice_ThrowsDomainException()
            {
                var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid());
                loan.MarkReturned(); // primera vez OK

                Action act = () => loan.MarkReturned(); // segunda vez

                act.Should().Throw<DomainException>()
                   .WithMessage("*ya fue devuelto*");
            }

            [Fact]
            public void Status_ComputedProperty_ReflectsReturnState()
            {
                var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid());
                loan.Status.Should().Be(LoanStatus.Borrowed);

                loan.MarkReturned();
                loan.Status.Should().Be(LoanStatus.Returned);
            }
        }
    }
}
