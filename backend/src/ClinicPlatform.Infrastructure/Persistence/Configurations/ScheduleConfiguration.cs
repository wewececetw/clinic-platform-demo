using ClinicPlatform.Domain.Entities;
using ClinicPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicPlatform.Infrastructure.Persistence.Configurations;

public class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
{
    public void Configure(EntityTypeBuilder<Schedule> builder)
    {
        builder.ToTable("schedules");

        builder.Property(x => x.Id).HasDefaultValueSql("(UUID())");
        builder.Property(x => x.DayOfWeekFlags).HasConversion<int>();
        builder.Property(x => x.TimeSlot).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(x => new { x.ClinicId, x.UserId });

        builder.HasOne(x => x.User).WithMany(x => x.Schedules)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Department).WithMany()
            .HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Room).WithMany()
            .HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Clinic).WithMany()
            .HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Restrict);
    }
}
