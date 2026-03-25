using ClinicPlatform.Domain.Enums;

namespace ClinicPlatform.Domain.Entities;

public class NotificationLog
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid? VisitId { get; set; }
    public Guid? PatientId { get; set; }
    public NotificationChannel Channel { get; set; }
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public NotificationStatus Status { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public Visit? Visit { get; set; }
    public Patient? Patient { get; set; }
}
