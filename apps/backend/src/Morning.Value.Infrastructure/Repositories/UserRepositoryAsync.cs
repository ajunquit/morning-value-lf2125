using Microsoft.EntityFrameworkCore;
using Morning.Value.Domain.Users.Entity;
using Morning.Value.Domain.Users.Interfaces;
using Morning.Value.Infrastructure.Persistences.Contexts;

namespace Morning.Value.Infrastructure.Repositories
{
    public class UserRepositoryAsync(AppDbContext dbContext) : RepositoryAsync<User>(dbContext), IUserRepositoryAsync
    {
        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
            => await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
            => await dbContext.Users.AnyAsync(u => u.Email == email, ct);
    }
}
