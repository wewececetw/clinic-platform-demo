namespace ClinicPlatform.Domain.Entities;

public class Department
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
