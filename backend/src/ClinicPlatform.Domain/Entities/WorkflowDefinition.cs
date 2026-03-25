namespace ClinicPlatform.Domain.Entities;

public class WorkflowDefinition
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
