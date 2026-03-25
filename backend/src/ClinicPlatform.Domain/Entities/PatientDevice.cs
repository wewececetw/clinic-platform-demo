namespace ClinicPlatform.Domain.Entities;

public class PatientDevice
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public string DeviceToken { get; set; } = null!;
    public string? P256dh { get; set; }
    public string? AuthKey { get; set; }
    public string? UserAgent { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
}
