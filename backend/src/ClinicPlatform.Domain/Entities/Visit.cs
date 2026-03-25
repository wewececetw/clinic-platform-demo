using ClinicPlatform.Domain.Enums;

namespace ClinicPlatform.Domain.Entities;

public class Visit
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }
    public Guid WorkflowDefinitionId { get; set; }
    public Guid? CurrentStepId { get; set; }
    public Guid? DoctorId { get; set; }
    public Guid? RoomId { get; set; }
    public int? QueueNumber { get; set; }
    public CheckinMethod CheckinMethod { get; set; }
    public VisitStatus Status { get; set; }
    public bool NeedsMedication { get; set; } = true;
    public DateTime CheckedInAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Appointment Appointment { get; set; } = null!;
    public WorkflowDefinition WorkflowDefinition { get; set; } = null!;
    public WorkflowStep? CurrentStep { get; set; }
    public User? Doctor { get; set; }
    public Room? Room { get; set; }
    public ICollection<VisitEvent> VisitEvents { get; set; } = new List<VisitEvent>();
    public Prescription? Prescription { get; set; }
    public QueueEntry? QueueEntry { get; set; }
}
