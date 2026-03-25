namespace ClinicPlatform.Domain.Entities;

public class Patient
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string? Phone { get; set; }
    public string? FullName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? NationalId { get; set; }
    public string? Notes { get; set; }
    public bool IsAnonymous { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public ICollection<PatientDevice> PatientDevices { get; set; } = new List<PatientDevice>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
