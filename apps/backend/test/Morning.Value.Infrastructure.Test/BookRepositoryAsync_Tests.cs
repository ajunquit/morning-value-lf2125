using FluentAssertions;
using Morning.Value.Domain.Books.Entity;
using Morning.Value.Infrastructure.Repositories;
using Morning.Value.Infrastructure.Test.Shared;

namespace Morning.Value.Infrastructure.Test
{
    public class BookRepositoryAsync_Tests : IClassFixture<SqliteDbFixture>
    {
        private readonly SqliteDbFixture _fx;
        private readonly BookRepositoryAsync _repo;

        public BookRepositoryAsync_Tests(SqliteDbFixture fx)
        {
            _fx = fx;
            _repo = new BookRepositoryAsync(_fx.Db);
        }

        private async Task ResetDbAsync()
        {
            _fx.Db.Books.RemoveRange(_fx.Db.Books);
            await _fx.Db.SaveChangesAsync();
        }

        [Fact]
        public async Task Insert_And_Get_Works()
        {
            await ResetDbAsync();

            var b = new Book("Clean Architecture", "Robert C. Martin", "Tech", 3);

            (await _repo.InsertAsync(b)).Should().BeTrue();
            await _fx.Db.SaveChangesAsync();

            var loaded = await _repo.GetAsync(b.Id);
            loaded.Should().NotBeNull();
            loaded!.Title.Should().Be("Clean Architecture");
            loaded.Author.Should().Be("Robert C. Martin");
            loaded.Genre.Should().Be("Tech");
            loaded.AvailableCopies.Should().Be(3);
        }

        [Fact]
        public async Task Update_Works()
        {
            await ResetDbAsync();

            var b = new Book("DDD", "Eric Evans", "Tech", 1);
            await _repo.InsertAsync(b);
            await _fx.Db.SaveChangesAsync();

            // Cambiamos algo (disponibilidad)
            b.ReturnOne(); // 1 -> 2
            (await _repo.UpdateAsync(b)).Should().BeTrue();
            await _fx.Db.SaveChangesAsync();

            var again = await _repo.GetAsync(b.Id);
            again!.AvailableCopies.Should().Be(2);
        }

        [Fact]
        public async Task Delete_Works()
        {
            await ResetDbAsync();

            var b = new Book("Refactoring", "Martin Fowler", "Tech", 1);
            await _repo.InsertAsync(b);
            await _fx.Db.SaveChangesAsync();

            (await _repo.DeleteAsync(b.Id)).Should().BeTrue();
            await _fx.Db.SaveChangesAsync();

            // Después de borrar, GetAsync debe lanzar:
            Func<Task> act = async () => await _repo.GetAsync(b.Id);
            await act.Should().ThrowAsync<KeyNotFoundException>();

            // Opcional: validar que ya no existan libros
            (await _repo.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task GetAll_And_Count_Work()
        {
            await ResetDbAsync();

            var data = new List<Book>
            {
                new("Clean Architecture", "Robert C. Martin", "Tech", 3),
                new("Clean Code", "Robert C. Martin", "Tech", 2),
                new("Refactoring", "Martin Fowler", "Tech", 1),
            };
            foreach (var b in data) await _repo.InsertAsync(b);
            await _fx.Db.SaveChangesAsync();

            var all = await _repo.GetAllAsync();
            var count = await _repo.CountAsync();

            all.Should().HaveCount(3);
            count.Should().Be(3);
        }

        [Fact]
        public async Task Search_FiltersByTitleAuthorGenre_Paginates_AndReturnsTotal()
        {
            await ResetDbAsync();

            var data = new List<Book>
            {
                new("Clean Architecture", "Robert C. Martin", "Tech", 3),
                new("Clean Code", "Robert C. Martin", "Tech", 2),
                new("Refactoring", "Martin Fowler", "Tech", 1),
                new("The Pragmatic Programmer", "Andrew Hunt", "Tech", 1),
            };
            foreach (var b in data) await _repo.InsertAsync(b);
            await _fx.Db.SaveChangesAsync();

            // Buscar "clean" => debería encontrar 2 (Architecture y Code)
            var (items1, total1) = await _repo.SearchAsync("clean", page: 1, pageSize: 1);
            total1.Should().Be(2);
            items1.Should().HaveCount(1);
            // Orden esperado por Título ascendente (si tu repo lo hace)
            items1[0].Title.Should().Be("Clean Architecture");

            var (items2, total2) = await _repo.SearchAsync("clean", page: 2, pageSize: 1);
            total2.Should().Be(2);
            items2.Should().HaveCount(1);
            items2[0].Title.Should().Be("Clean Code");

            // Buscar por Autor
            var (items3, total3) = await _repo.SearchAsync("fowler", page: 1, pageSize: 10);
            total3.Should().Be(1);
            items3.Single().Title.Should().Be("Refactoring");

            // Buscar por Género (si también filtras por Genre)
            var (items4, total4) = await _repo.SearchAsync("tech", page: 1, pageSize: 10);
            total4.Should().Be(4);
            items4.Should().HaveCount(4);
        }
    }
}
