namespace ClinicPlatform.Domain.Entities;

public class WorkflowStep
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid WorkflowDefinitionId { get; set; }
    public string StepCode { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public int StepOrder { get; set; }
    public string? RequiredRole { get; set; }
    public bool IsSkippable { get; set; } = false;
    public bool AutoAdvance { get; set; } = false;
    public DateTime CreatedAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public WorkflowDefinition WorkflowDefinition { get; set; } = null!;
    public ICollection<WorkflowTransition> OutgoingTransitions { get; set; } = new List<WorkflowTransition>();
    public ICollection<WorkflowTransition> IncomingTransitions { get; set; } = new List<WorkflowTransition>();
}
