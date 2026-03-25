namespace ClinicPlatform.Domain.Entities;

public class OtpVerification
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string Phone { get; set; } = null!;
    public string OtpCode { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime CreatedAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
}
