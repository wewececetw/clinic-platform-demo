using ClinicPlatform.Domain.Enums;

namespace ClinicPlatform.Domain.Entities;

public class Prescription
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid VisitId { get; set; }
    public Guid DoctorId { get; set; }
    public PrescriptionStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime PrescribedAt { get; set; }
    public DateTime? SentToPharmacyAt { get; set; }
    public DateTime? DispensedAt { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public Visit Visit { get; set; } = null!;
    public User Doctor { get; set; } = null!;
    public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
}
