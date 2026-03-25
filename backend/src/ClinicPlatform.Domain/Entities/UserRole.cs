namespace ClinicPlatform.Domain.Entities;

public class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Guid ClinicId { get; set; }

    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
    public Clinic Clinic { get; set; } = null!;
}
