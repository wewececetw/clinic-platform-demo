namespace ClinicPlatform.Domain.Entities;

public class Medication
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Unit { get; set; } = null!;
    public string? DefaultDosage { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
}
