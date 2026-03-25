using ClinicPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicPlatform.Infrastructure.Persistence.Configurations;

public class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.ToTable("prescription_items");

        builder.Property(x => x.Id).HasDefaultValueSql("(UUID())");
        builder.Property(x => x.Dosage).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Frequency).IsRequired().HasMaxLength(50);

        builder.HasIndex(x => x.PrescriptionId);

        builder.HasOne(x => x.Prescription).WithMany(x => x.Items)
            .HasForeignKey(x => x.PrescriptionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Medication).WithMany()
            .HasForeignKey(x => x.MedicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Clinic).WithMany()
            .HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Restrict);
    }
}
