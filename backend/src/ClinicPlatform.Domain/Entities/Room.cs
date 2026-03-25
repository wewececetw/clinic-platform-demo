using ClinicPlatform.Domain.Enums;

namespace ClinicPlatform.Domain.Entities;

public class Room
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid DepartmentId { get; set; }
    public string Name { get; set; } = null!;
    public RoomType RoomType { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public Department Department { get; set; } = null!;
}
