using FluentAssertions;
using Morning.Value.Domain.Books.Entity;
using Morning.Value.Domain.Loans.Entity;
using Morning.Value.Domain.Loans.Enums;
using Morning.Value.Domain.Users.Entity;
using Morning.Value.Infrastructure.Repositories;
using Morning.Value.Infrastructure.Test.Shared;

namespace Morning.Value.Infrastructure.Test
{
    public class LoanRepositoryAsync_Tests : IClassFixture<SqliteDbFixture>
    {
        private readonly SqliteDbFixture _fx;
        private readonly LoanRepositoryAsync _repo;

        public LoanRepositoryAsync_Tests(SqliteDbFixture fx)
        {
            _fx = fx;
            _repo = new LoanRepositoryAsync(_fx.Db);
            // usuario “actual” para auditoría (opcional)
            _fx.CurrentUser.Setup(x => x.UserId).Returns("test-user");
        }

        private async Task ResetDbAsync()
        {
            // Respeta FK: primero Loans, luego Books/Users
            _fx.Db.Loans.RemoveRange(_fx.Db.Loans);
            _fx.Db.Books.RemoveRange(_fx.Db.Books);
            _fx.Db.Users.RemoveRange(_fx.Db.Users);
            await _fx.Db.SaveChangesAsync();
        }

        private async Task<(Guid user1, Guid user2, Guid bCleanArch, Guid bCleanCode, Guid bRefactoring)> SeedBasicAsync()
        {
            var u1 = new User("Alejo", "alejo@acme.com", "HASH");
            var u2 = new User("Reader", "reader@acme.com", "HASH");
            _fx.Db.Users.AddRange(u1, u2);

            var b1 = new Book("Clean Architecture", "Robert C. Martin", "Tech", 5);
            var b2 = new Book("Clean Code", "Robert C. Martin", "Tech", 2);
            var b3 = new Book("Refactoring", "Martin Fowler", "Tech", 1);
            _fx.Db.Books.AddRange(b1, b2, b3);

            await _fx.Db.SaveChangesAsync();

            return (u1.Id, u2.Id, b1.Id, b2.Id, b3.Id);
        }

        [Fact]
        public async Task Insert_Update_Get_Works()
        {
            await ResetDbAsync();
            var (user, _, book, _, _) = await SeedBasicAsync();

            var loan = Loan.Create(user, book);
            (await _repo.InsertAsync(loan)).Should().BeTrue();
            await _fx.Db.SaveChangesAsync();

            var loaded = await _repo.GetAsync(loan.Id);
            loaded.Should().NotBeNull();
            loaded!.UserId.Should().Be(user);
            loaded.BookId.Should().Be(book);
            loaded.ReturnDateUtc.Should().BeNull();

            // marcar devuelto y actualizar
            loaded.MarkReturned();
            (await _repo.UpdateAsync(loaded)).Should().BeTrue();
            await _fx.Db.SaveChangesAsync();

            var again = await _repo.GetAsync(loan.Id);
            again!.ReturnDateUtc.Should().NotBeNull();
        }

        [Fact]
        public async Task GetActiveByUserAndBook_Returns_OnlyActiveLoan()
        {
            await ResetDbAsync();
            var (u1, u2, bCleanArch, _, _) = await SeedBasicAsync();

            // u1: uno activo + uno devuelto
            var l1 = Loan.Create(u1, bCleanArch);
            var l2 = Loan.Create(u1, bCleanArch);
            l2.MarkReturned();

            // u2: activo (otro usuario, mismo libro)
            var l3 = Loan.Create(u2, bCleanArch);

            _fx.Db.Loans.AddRange(l1, l2, l3);
            await _fx.Db.SaveChangesAsync();

            var active = await _repo.GetActiveByUserAndBookAsync(u1, bCleanArch);
            active.Should().NotBeNull();
            active!.UserId.Should().Be(u1);
            active.BookId.Should().Be(bCleanArch);
            active.ReturnDateUtc.Should().BeNull();

            // Para u2
            var active2 = await _repo.GetActiveByUserAndBookAsync(u2, bCleanArch);
            active2.Should().NotBeNull();
            active2!.UserId.Should().Be(u2);
        }

        [Fact]
        public async Task GetActiveByUserAndBook_Returns_Null_When_NoActive()
        {
            await ResetDbAsync();
            var (u1, _, bCleanArch, _, _) = await SeedBasicAsync();

            var l = Loan.Create(u1, bCleanArch);
            l.MarkReturned();
            _fx.Db.Loans.Add(l);
            await _fx.Db.SaveChangesAsync();

            var active = await _repo.GetActiveByUserAndBookAsync(u1, bCleanArch);
            active.Should().BeNull();
        }
    }
}
