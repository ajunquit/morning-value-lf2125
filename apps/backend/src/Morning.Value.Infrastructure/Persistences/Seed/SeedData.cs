using Microsoft.EntityFrameworkCore;
using Morning.Value.Application.Common.Services;
using Morning.Value.Domain.Users.Entity;
using Morning.Value.Domain.Users.Enums;
using Morning.Value.Infrastructure.Persistences.Contexts;

namespace Morning.Value.Infrastructure.Persistences.Seed
{
    public static class SeedData
    {
        public static async Task SeedInitialDataAsync(AppDbContext db, IPasswordHasher hasher)
        {
            var seeds = new[]
            {
                new User("Administrator", "admin@domain.com",  hasher.Hash("admin123"),  RoleType.Admin),
                new User("Reader","reader@domain.com", hasher.Hash("reader123"), RoleType.Reader)
            };

            var targetEmails = seeds.Select(s => s.Email).ToArray();

            var existing = await db.Set<User>()
                .AsNoTracking()
                .Where(u => targetEmails.Contains(u.Email))
                .Select(u => u.Email)
                .ToListAsync();

            var toCreate = new List<User>();
            foreach (var s in seeds)
            {
                if (existing.Contains(s.Email))
                    continue;

                toCreate.Add(new User(
                    name: s.Name,
                    email: s.Email,
                    passwordHash: s.PasswordHash,
                    role: s.Role));
            }

            if (toCreate.Count == 0)
                return;

            await db.Set<User>().AddRangeAsync(toCreate);
            await db.SaveChangesAsync();
        }
    }
}
