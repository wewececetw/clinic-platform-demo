namespace ClinicPlatform.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
