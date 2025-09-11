using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Morning.Value.Application.Common.Services;
using Morning.Value.Infrastructure.Persistences.Contexts;
using Morning.Value.Infrastructure.Persistences.Interceptors;
using System.Data.Common;

namespace Morning.Value.Infrastructure.Test.Shared
{
    public class SqliteDbFixture : IDisposable
    {
        private readonly DbConnection _conn;
        public AppDbContext Db { get; }

        public Mock<ICurrentUserService> CurrentUser { get; } = new();

        public SqliteDbFixture()
        {
            _conn = new SqliteConnection("DataSource=:memory:");
            _conn.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_conn)
                .AddInterceptors(new AuditingSaveChangesInterceptor(CurrentUser.Object))
                .EnableSensitiveDataLogging()
                .Options;

            Db = new AppDbContext(options);
            Db.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Db.Dispose();
            _conn.Dispose();
        }
    }
}
