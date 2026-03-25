using ClinicPlatform.Domain.Enums;

namespace ClinicPlatform.Domain.Entities;

public class Appointment
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid? DoctorId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string? QrCodeToken { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public User? Doctor { get; set; }
}
