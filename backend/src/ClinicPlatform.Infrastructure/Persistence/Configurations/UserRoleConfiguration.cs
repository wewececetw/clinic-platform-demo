using ClinicPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicPlatform.Infrastructure.Persistence.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");

        builder.HasKey(x => new { x.UserId, x.RoleId });

        builder.HasOne(x => x.User).WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role).WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Clinic).WithMany()
            .HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ClinicId);
    }
}
