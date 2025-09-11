using Morning.Value.Application.Users.Dtos;
using Morning.Value.Domain.Users.Enums;

namespace Morning.Value.Application.Users.Services
{
    public interface IAuthAppService
    {
        Task<RegisterResult> SignUpAsync(string name, string email, string password, RoleType role, CancellationToken ct = default);
        Task<AuthResult> SignInAsync(string email, string password, CancellationToken ct = default);
    }
}
