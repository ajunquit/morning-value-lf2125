namespace Morning.Value.Domain.Common.Entities
{
    public abstract class AuditableEntity : Entity
    {
        public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;
        public string? CreatedBy { get; protected set; }
        public DateTime? ModifiedAtUtc { get; protected set; }
        public string? ModifiedBy { get; protected set; }

        public void SetCreated(string? user) => CreatedBy = user;
        public void SetModified(string? user)
        {
            ModifiedBy = user;
            ModifiedAtUtc = DateTime.UtcNow;
        }
    }
}
