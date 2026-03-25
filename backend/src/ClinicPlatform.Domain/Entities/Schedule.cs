using ClinicPlatform.Domain.Enums;

namespace ClinicPlatform.Domain.Entities;

public class Schedule
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid UserId { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid? RoomId { get; set; }
    public DayOfWeekFlag DayOfWeekFlags { get; set; }
    public TimeSlot TimeSlot { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public User User { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public Room? Room { get; set; }
}
