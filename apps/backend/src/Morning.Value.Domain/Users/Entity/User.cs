using Morning.Value.Domain.Common.Entities;
using Morning.Value.Domain.Exceptions;
using Morning.Value.Domain.Users.Enums;

namespace Morning.Value.Domain.Users.Entity
{
    public class User: AuditableEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;

        public RoleType Role { get; private set; } = RoleType.Reader;

        private User() { }

        public User(string name, string email, string passwordHash, RoleType role = RoleType.Reader)
        {
            SetName(name);
            SetEmail(email);
            SetPasswordHash(passwordHash);
            Role = role;
        }

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("El nombre es obligatorio.");
            Name = name.Trim();
        }

        public void SetEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new DomainException("El email es obligatorio.");
            Email = email.Trim();
        }

        public void SetPasswordHash(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash)) throw new DomainException("Password hash inválido.");
            PasswordHash = hash;
        }

        public void SetRole(RoleType role) => Role = role;

    }
}
