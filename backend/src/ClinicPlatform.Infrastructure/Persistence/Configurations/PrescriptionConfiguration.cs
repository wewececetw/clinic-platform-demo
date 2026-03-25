using ClinicPlatform.Domain.Entities;
using ClinicPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicPlatform.Infrastructure.Persistence.Configurations;

public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.ToTable("prescriptions");

        builder.Property(x => x.Id).HasDefaultValueSql("(UUID())");
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

        builder.HasIndex(x => new { x.ClinicId, x.Status });
        builder.HasIndex(x => new { x.ClinicId, x.VisitId });

        builder.HasOne(x => x.Visit).WithOne(x => x.Prescription)
            .HasForeignKey<Prescription>(x => x.VisitId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Doctor).WithMany()
            .HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Clinic).WithMany()
            .HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Restrict);
    }
}
