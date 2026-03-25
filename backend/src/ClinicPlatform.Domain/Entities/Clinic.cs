namespace ClinicPlatform.Domain.Entities;

public class Clinic
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? SettingsJson { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Patient> Patients { get; set; } = new List<Patient>();
    public ICollection<Department> Departments { get; set; } = new List<Department>();
    public ICollection<WorkflowDefinition> WorkflowDefinitions { get; set; } = new List<WorkflowDefinition>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
