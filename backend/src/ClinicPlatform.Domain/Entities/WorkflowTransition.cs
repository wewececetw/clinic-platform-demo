namespace ClinicPlatform.Domain.Entities;

public class WorkflowTransition
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid WorkflowDefinitionId { get; set; }
    public Guid FromStepId { get; set; }
    public Guid ToStepId { get; set; }
    public string? ConditionJson { get; set; }
    public int Priority { get; set; } = 0;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public WorkflowDefinition WorkflowDefinition { get; set; } = null!;
    public WorkflowStep FromStep { get; set; } = null!;
    public WorkflowStep ToStep { get; set; } = null!;
}
