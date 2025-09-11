using Morning.Value.Application.Common.Services;
using Morning.Value.Application.Users.Dtos;
using Morning.Value.Domain.Common.Interfaces;
using Morning.Value.Domain.Users.Entity;
using Morning.Value.Domain.Users.Enums;

namespace Morning.Value.Application.Users.Services
{
    public class AuthAppService : IAuthAppService
    {
        private readonly IUnitOfWorkAsync _uow;
        private readonly IPasswordHasher _hasher;

        public AuthAppService(IUnitOfWorkAsync uow, IPasswordHasher hasher)
        {
            _uow = uow;
            _hasher = hasher;
        }

        public async Task<RegisterResult> SignUpAsync(string name, string email, string password, RoleType role, CancellationToken ct = default)
        {
            // email único
            if (await _uow.UserRepository.EmailExistsAsync(email, ct))
                return new RegisterResult { Success = false, Error = "El correo ya está registrado." };

            var hash = _hasher.Hash(password);
            var user = new User(name, email, hash, role);

            await _uow.UserRepository.InsertAsync(user);
            await _uow.SaveChangesAsync(ct);

            return new RegisterResult { Success = true, UserId = user.Id };
        }

        public async Task<AuthResult> SignInAsync(string email, string password, CancellationToken ct = default)
        {
            var user = await _uow.UserRepository.GetByEmailAsync(email, ct);
            if (user is null)
                return new AuthResult { Success = false, Error = "Usuario o contraseña inválidos." };

            var ok = _hasher.Verify(user.PasswordHash, password);
            if (!ok)
                return new AuthResult { Success = false, Error = "Usuario o contraseña inválidos." };

            return new AuthResult
            {
                Success = true,
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };
        }
    }
}
