namespace ClinicPlatform.Domain.Entities;

public class PrescriptionItem
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid PrescriptionId { get; set; }
    public Guid MedicationId { get; set; }
    public string Dosage { get; set; } = null!;
    public string Frequency { get; set; } = null!;
    public int DurationDays { get; set; }
    public int Quantity { get; set; }
    public string? Instructions { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public Prescription Prescription { get; set; } = null!;
    public Medication Medication { get; set; } = null!;
}
