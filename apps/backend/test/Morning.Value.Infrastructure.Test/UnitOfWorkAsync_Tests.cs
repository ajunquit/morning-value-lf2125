using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Morning.Value.Domain.Books.Entity;
using Morning.Value.Domain.Users.Entity;
using Morning.Value.Infrastructure.Repositories;
using Morning.Value.Infrastructure.Test.Shared;

namespace Morning.Value.Infrastructure.Test
{
    public class UnitOfWorkAsync_Tests : IClassFixture<SqliteDbFixture>
    {
        private readonly SqliteDbFixture _fx;

        public UnitOfWorkAsync_Tests(SqliteDbFixture fx)
        {
            _fx = fx;
            _fx.CurrentUser.Setup(x => x.UserId).Returns("test-user");
        }

        private UnitOfWorkAsync CreateUow()
        {
            var userRepo = new UserRepositoryAsync(_fx.Db);
            var loanRepo = new LoanRepositoryAsync(_fx.Db);
            var bookRepo = new BookRepositoryAsync(_fx.Db);

            return new UnitOfWorkAsync(userRepo, loanRepo, bookRepo, _fx.Db);
        }

        private async Task ResetDbAsync()
        {
            _fx.Db.Loans.RemoveRange(_fx.Db.Loans);
            _fx.Db.Books.RemoveRange(_fx.Db.Books);
            _fx.Db.Users.RemoveRange(_fx.Db.Users);
            await _fx.Db.SaveChangesAsync();
        }

        [Fact]
        public void Repositories_Are_Not_Null()
        {
            var uow = CreateUow();

            uow.BookRepository.Should().NotBeNull();
            uow.UserRepository.Should().NotBeNull();
            uow.LoanRepository.Should().NotBeNull();
        }

        [Fact]
        public async Task SaveChangesAsync_NoChanges_ReturnsZero()
        {
            var uow = CreateUow();

            var rows = await uow.SaveChangesAsync();
            rows.Should().Be(0);
        }

        [Fact]
        public async Task SaveChangesAsync_Persists_Multiple_Inserts_With_Single_Commit()
        {
            await ResetDbAsync();
            var uow = CreateUow();

            await uow.BookRepository.InsertAsync(new Book("Clean Architecture", "Robert C. Martin", "Tech", 3));
            await uow.UserRepository.InsertAsync(new User("Alejo", "alejo@acme.com", "HASH"));

            var affected = await uow.SaveChangesAsync();

            affected.Should().BeGreaterThanOrEqualTo(2);
            (await _fx.Db.Books.CountAsync()).Should().Be(1);
            (await _fx.Db.Users.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task Repositories_Share_Same_DbContext_UoW_Semantics_Hold()
        {
            await ResetDbAsync();
            var uow = CreateUow();

            await uow.BookRepository.InsertAsync(new Book("A Book", "Author", "Tech", 1));
            await uow.BookRepository.InsertAsync(new Book("B Book", "Author", "Tech", 1));
            await uow.UserRepository.InsertAsync(new User("User", "user@acme.com", "HASH"));

            var affected = await uow.SaveChangesAsync();

            affected.Should().BeGreaterThanOrEqualTo(3);
            (await _fx.Db.Books.CountAsync()).Should().Be(2);
            (await _fx.Db.Users.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task SaveChangesAsync_After_Update_Persists_Modifications()
        {
            await ResetDbAsync();
            var uow = CreateUow();

            var book = new Book("Refactoring", "Martin Fowler", "Tech", 1);
            await uow.BookRepository.InsertAsync(book);
            await uow.SaveChangesAsync();

            book.ReturnOne(); // 1 -> 2
            await uow.BookRepository.UpdateAsync(book);

            var affected = await uow.SaveChangesAsync();
            affected.Should().BeGreaterThanOrEqualTo(1);

            var loaded = await uow.BookRepository.GetAsync(book.Id);
            loaded.AvailableCopies.Should().Be(2);
        }
    }
}
