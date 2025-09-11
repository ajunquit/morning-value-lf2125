using Morning.Value.Domain.Common.Interfaces;
using Morning.Value.Domain.Users.Entity;

namespace Morning.Value.Domain.Users.Interfaces
{
    public interface IUserRepositoryAsync : IRepositoryAsync<User>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    }
}
