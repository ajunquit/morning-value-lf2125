namespace Morning.Value.Application.Users.Dtos
{
    public sealed class RegisterResult
    {
        public bool Success { get; init; }
        public string? Error { get; init; }
        public Guid? UserId { get; init; }
    }
}
