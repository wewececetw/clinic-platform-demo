using ClinicPlatform.Domain.Enums;

namespace ClinicPlatform.Domain.Entities;

public class VisitEvent
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid VisitId { get; set; }
    public Guid? FromStepId { get; set; }
    public Guid? ToStepId { get; set; }
    public TriggerType TriggerType { get; set; }
    public Guid? TriggeredByUserId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public Visit Visit { get; set; } = null!;
    public WorkflowStep? FromStep { get; set; }
    public WorkflowStep? ToStep { get; set; }
    public User? TriggeredByUser { get; set; }
}
