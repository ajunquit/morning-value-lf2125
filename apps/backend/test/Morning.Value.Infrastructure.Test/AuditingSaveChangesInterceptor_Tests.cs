using FluentAssertions;
using Morning.Value.Domain.Books.Entity;
using Morning.Value.Infrastructure.Test.Shared;

namespace Morning.Value.Infrastructure.Test
{
    public class AuditingSaveChangesInterceptor_Tests : IClassFixture<SqliteDbFixture>
    {
        private readonly SqliteDbFixture _fx;

        public AuditingSaveChangesInterceptor_Tests(SqliteDbFixture fx)
        {
            _fx = fx;

            // El interceptor toma: UserId ?? Email ?? UserName
            _fx.CurrentUser.Setup(x => x.UserId).Returns("user-123");
            _fx.CurrentUser.Setup(x => x.Email).Returns("alejo@acme.com");
            _fx.CurrentUser.Setup(x => x.UserName).Returns("Alejo");
        }

        [Fact]
        public async Task AlCrear_SetCreatedAtUtc_Y_CreatedBy_PeroNoModified()
        {
            // arrange
            var t0 = DateTime.UtcNow;
            var book = new Book("Clean Architecture", "Robert C. Martin", "Tech", 3);

            // act
            _fx.Db.Books.Add(book);
            await _fx.Db.SaveChangesAsync();

            // assert
            book.CreatedAtUtc.Should().BeOnOrAfter(t0).And.BeOnOrBefore(DateTime.UtcNow);
            book.CreatedBy.Should().Be("user-123"); // toma UserId primero

            book.ModifiedAtUtc.Should().BeNull();
            book.ModifiedBy.Should().BeNull();
        }

        [Fact]
        public async Task AlModificar_NoTocaCreated_PeroSeteaModified()
        {
            // arrange: crear primero
            var book = new Book("DDD", "Eric Evans", "Tech", 1);
            _fx.Db.Books.Add(book);
            await _fx.Db.SaveChangesAsync();

            var createdAt = book.CreatedAtUtc;
            var createdBy = book.CreatedBy;

            // act: cambiar algo para marcar Modified
            book.ReturnOne(); // incrementa disponibilidad => marca Modified
            await _fx.Db.SaveChangesAsync();

            // assert
            book.CreatedAtUtc.Should().Be(createdAt);
            book.CreatedBy.Should().Be(createdBy);

            book.ModifiedAtUtc.Should().NotBeNull();
            book.ModifiedBy.Should().Be("user-123");
        }
    }
}
