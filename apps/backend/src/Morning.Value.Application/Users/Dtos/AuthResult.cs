using Morning.Value.Domain.Users.Enums;

namespace Morning.Value.Application.Users.Dtos
{
    public sealed class AuthResult
    {
        public bool Success { get; init; }
        public string? Error { get; init; }
        public Guid? UserId { get; init; }
        public string? Name { get; init; }
        public string? Email { get; init; }
        public RoleType? Role { get; init; }
    }
}
