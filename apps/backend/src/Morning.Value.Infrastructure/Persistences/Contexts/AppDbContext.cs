using Microsoft.EntityFrameworkCore;
using Morning.Value.Domain.Books.Entity;
using Morning.Value.Domain.Common.Interfaces;
using Morning.Value.Domain.Loans.Entity;
using Morning.Value.Domain.Users.Entity;

namespace Morning.Value.Infrastructure.Persistences.Contexts
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        public DbSet<Book> Books => Set<Book>();
        public DbSet<Loan> Loans => Set<Loan>();
        public DbSet<User> Users => Set<User>();
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
