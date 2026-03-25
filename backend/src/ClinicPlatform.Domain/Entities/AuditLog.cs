namespace ClinicPlatform.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid? UserId { get; set; }
    public string EntityType { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public User? User { get; set; }
}
