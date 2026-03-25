using ClinicPlatform.Domain.Enums;

namespace ClinicPlatform.Domain.Entities;

public class QueueConfig
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public QueueType QueueType { get; set; }
    public string Name { get; set; } = null!;
    public int PriorityWeight { get; set; } = 0;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
}
