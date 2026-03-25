using ClinicPlatform.Domain.Entities;
using ClinicPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicPlatform.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments");

        builder.Property(x => x.Id).HasDefaultValueSql("(UUID())");
        builder.Property(x => x.QrCodeToken).HasMaxLength(100);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

        builder.HasIndex(x => x.QrCodeToken).IsUnique();
        builder.HasIndex(x => new { x.ClinicId, x.AppointmentDate });
        builder.HasIndex(x => new { x.ClinicId, x.PatientId });

        builder.HasOne(x => x.Patient).WithMany(x => x.Appointments)
            .HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Department).WithMany()
            .HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Doctor).WithMany()
            .HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Clinic).WithMany(x => x.Appointments)
            .HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Restrict);
    }
}
