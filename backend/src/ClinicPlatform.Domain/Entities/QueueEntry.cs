using ClinicPlatform.Domain.Enums;

namespace ClinicPlatform.Domain.Entities;

public class QueueEntry
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid VisitId { get; set; }
    public QueueType QueueType { get; set; }
    public int QueueNumber { get; set; }
    public int Priority { get; set; } = 0;
    public QueueEntryStatus Status { get; set; }
    public DateTime? CalledAt { get; set; }
    public DateTime? SkippedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public Visit Visit { get; set; } = null!;
}
