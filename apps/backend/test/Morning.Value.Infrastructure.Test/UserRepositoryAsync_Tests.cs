using FluentAssertions;
using Morning.Value.Domain.Users.Entity;
using Morning.Value.Infrastructure.Repositories;
using Morning.Value.Infrastructure.Test.Shared;

namespace Morning.Value.Infrastructure.Test
{
    public class UserRepositoryAsync_Tests : IClassFixture<SqliteDbFixture>
    {
        private readonly SqliteDbFixture _fx;
        private readonly UserRepositoryAsync _repo;

        public UserRepositoryAsync_Tests(SqliteDbFixture fx)
        {
            _fx = fx;
            _repo = new UserRepositoryAsync(_fx.Db);

            // usuario “actual” para auditoría (si usas interceptor)
            _fx.CurrentUser.Setup(x => x.UserId).Returns("test-user");
        }

        private async Task ResetDbAsync()
        {
            // Respeta FK: primero Loans, luego Users
            _fx.Db.Loans.RemoveRange(_fx.Db.Loans);
            _fx.Db.Users.RemoveRange(_fx.Db.Users);
            await _fx.Db.SaveChangesAsync();
        }

        [Fact]
        public async Task Insert_And_Get_Works()
        {
            await ResetDbAsync();

            var u = new User("Alejo", "alejo@acme.com", "HASH");
            (await _repo.InsertAsync(u)).Should().BeTrue();
            await _fx.Db.SaveChangesAsync();

            var loaded = await _repo.GetAsync(u.Id);
            loaded.Should().NotBeNull();
            loaded!.Name.Should().Be("Alejo");
            loaded.Email.Should().Be("alejo@acme.com");
            loaded.PasswordHash.Should().Be("HASH");
        }

        [Fact]
        public async Task Update_Works()
        {
            await ResetDbAsync();

            var u = new User("Reader", "reader@acme.com", "HASH");
            await _repo.InsertAsync(u);
            await _fx.Db.SaveChangesAsync();

            u.SetName("Reader Updated");
            (await _repo.UpdateAsync(u)).Should().BeTrue();
            await _fx.Db.SaveChangesAsync();

            var again = await _repo.GetAsync(u.Id);
            again!.Name.Should().Be("Reader Updated");
        }

        [Fact]
        public async Task Delete_Works()
        {
            await ResetDbAsync();

            var u = new User("ToRemove", "remove@acme.com", "HASH");
            await _repo.InsertAsync(u);
            await _fx.Db.SaveChangesAsync();

            (await _repo.DeleteAsync(u.Id)).Should().BeTrue();
            await _fx.Db.SaveChangesAsync();

            // Si tu GetAsync lanza excepción cuando no encuentra, esperamos la excepción:
            Func<Task> act = async () => await _repo.GetAsync(u.Id);
            await act.Should().ThrowAsync<KeyNotFoundException>();

            (await _repo.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task EmailExists_And_GetByEmail_Work()
        {
            await ResetDbAsync();

            var u = new User("Alejo", "alejo@acme.com", "HASH");
            await _repo.InsertAsync(u);
            await _fx.Db.SaveChangesAsync();

            (await _repo.EmailExistsAsync("alejo@acme.com")).Should().BeTrue();
            (await _repo.EmailExistsAsync("no@exists.com")).Should().BeFalse();

            var byEmail = await _repo.GetByEmailAsync("alejo@acme.com");
            byEmail.Should().NotBeNull();
            byEmail!.Id.Should().Be(u.Id);
        }

        [Fact]
        public async Task GetAll_And_Count_Work()
        {
            await ResetDbAsync();

            await _repo.InsertAsync(new User("U1", "u1@acme.com", "H"));
            await _repo.InsertAsync(new User("U2", "u2@acme.com", "H"));
            await _fx.Db.SaveChangesAsync();

            var all = await _repo.GetAllAsync();
            var count = await _repo.CountAsync();

            all.Should().HaveCount(2);
            count.Should().Be(2);
            all.Select(x => x.Email).Should().Contain(new[] { "u1@acme.com", "u2@acme.com" });
        }
    }
}
