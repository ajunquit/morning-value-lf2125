using FluentAssertions;
using Moq;
using Morning.Value.Application.Common.Dtos;
using Morning.Value.Application.Loans.Dtos;
using Morning.Value.Application.Loans.Services;
using Morning.Value.Domain.Common.Interfaces;
using Morning.Value.Domain.Loans.Entity;
using Morning.Value.Domain.Loans.Enums;
using Morning.Value.Domain.Loans.Interfaces;

namespace Morning.Value.Application.Test.Loans
{
    internal sealed class LoanHistoryRowFake
    {
        public Guid LoanId { get; init; }
        public string BookTitle { get; init; } = string.Empty;
        public string Author { get; init; } = string.Empty;
        public string Genre { get; init; } = string.Empty;
        public DateTime LoanDateUtc { get; init; }
        public DateTime? ReturnDateUtc { get; init; }
    }

    public class LoanAppService_GetHistoryByUserAsync_Tests
    {
        private readonly Mock<IUnitOfWorkAsync> _uow = new();
        private readonly Mock<ILoanRepositoryAsync> _loanRepo = new();

        private readonly LoanAppService _sut;

        public LoanAppService_GetHistoryByUserAsync_Tests()
        {
            _uow.SetupGet(x => x.LoanRepository).Returns(_loanRepo.Object);
            _sut = new LoanAppService(_uow.Object);
        }

        [Fact]
        public async Task GetHistory_ReturnsPagedResult_WithMappedItems_AndTotals()
        {
            var userId = Guid.NewGuid();
            var page = 2;
            var pageSize = 3;
            var q = "clean";
            LoanStatus? status = LoanStatus.Borrowed;

            var rows = new List<LoanHistoryRow>
            {
                new LoanHistoryRow
                {
                    LoanId = Guid.NewGuid(),
                    BookTitle = "Clean Architecture",
                    Author = "Robert C. Martin",
                    Genre = "Tech",
                    LoanDateUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    ReturnDateUtc = null
                },
                new LoanHistoryRow
                {
                    LoanId = Guid.NewGuid(),
                    BookTitle = "DDD",
                    Author = "Evans",
                    Genre = "Tech",
                    LoanDateUtc = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                    ReturnDateUtc = new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc)
                }
            };

            var tuple = (Items: (IReadOnlyList<LoanHistoryRow>)rows.AsReadOnly(), Total: 25);

            _loanRepo
                .Setup(r => r.GetHistoryByUserAsync(userId, q, status, page, pageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tuple);

            // act
            PagedResult<LoanHistoryItemResponse> result =
                await _sut.GetHistoryByUserAsync(userId, q, status, page, pageSize);

            // assert
            result.PageIndex.Should().Be(page);
            result.PageSize.Should().Be(pageSize);
            result.TotalCount.Should().Be(25);
            result.Query.Should().Be(q);
            result.StatusFilter.Should().Be(status.ToString()!.ToLower());

            result.Items.Should().HaveCount(2);
            var first = result.Items[0];
            first.BookTitle.Should().Be("Clean Architecture");
            first.Author.Should().Be("Robert C. Martin");
            first.Genre.Should().Be("Tech");
            first.LoanDate.Should().Be(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            first.ReturnDate.Should().BeNull();
            first.Status.Should().Be(LoanStatus.Borrowed);

            var second = result.Items[1];
            second.Status.Should().Be(LoanStatus.Returned);

            _loanRepo.Verify(r =>
                r.GetHistoryByUserAsync(userId, q, status, page, pageSize, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [InlineData(0, 0, 1, 10)] // page <= 0 => 1 ; pageSize <= 0 => 10
        [InlineData(-3, -5, 1, 10)]
        public async Task GetHistory_NormalizesPageAndPageSize(int page, int pageSize, int expectedPage, int expectedSize)
        {
            var userId = Guid.NewGuid();

            var tuple = (Items: (IReadOnlyList<LoanHistoryRow>)Array.Empty<LoanHistoryRow>(), Total: 0);

            _loanRepo
                .Setup(r => r.GetHistoryByUserAsync(userId, null, null, expectedPage, expectedSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tuple);

            var result = await _sut.GetHistoryByUserAsync(userId, null, null, page, pageSize);

            result.PageIndex.Should().Be(expectedPage);
            result.PageSize.Should().Be(expectedSize);
            _loanRepo.Verify(r => r.GetHistoryByUserAsync(userId, null, null, expectedPage, expectedSize, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
