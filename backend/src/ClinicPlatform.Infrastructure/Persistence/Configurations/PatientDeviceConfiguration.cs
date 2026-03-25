using ClinicPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicPlatform.Infrastructure.Persistence.Configurations;

public class PatientDeviceConfiguration : IEntityTypeConfiguration<PatientDevice>
{
    public void Configure(EntityTypeBuilder<PatientDevice> builder)
    {
        builder.ToTable("patient_devices");

        builder.Property(x => x.Id).HasDefaultValueSql("(UUID())");
        builder.Property(x => x.DeviceToken).IsRequired().HasMaxLength(500);
        builder.Property(x => x.P256dh).HasMaxLength(500);
        builder.Property(x => x.AuthKey).HasMaxLength(500);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

        builder.HasIndex(x => new { x.ClinicId, x.PatientId });

        builder.HasOne(x => x.Patient).WithMany(x => x.PatientDevices)
            .HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Clinic).WithMany()
            .HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Restrict);
    }
}
