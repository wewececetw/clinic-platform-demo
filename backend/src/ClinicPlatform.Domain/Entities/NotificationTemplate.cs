using ClinicPlatform.Domain.Enums;

namespace ClinicPlatform.Domain.Entities;

public class NotificationTemplate
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string StepCode { get; set; } = null!;
    public NotificationChannel Channel { get; set; }
    public string TitleTemplate { get; set; } = null!;
    public string BodyTemplate { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
}
