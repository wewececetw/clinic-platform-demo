using ClinicPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicPlatform.Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

        builder.Property(x => x.Id).HasDefaultValueSql("(UUID())");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(20);
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasIndex(x => new { x.ClinicId, x.Code }).IsUnique();

        builder.HasOne(x => x.Clinic).WithMany(x => x.Departments)
            .HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Restrict);
    }
}
