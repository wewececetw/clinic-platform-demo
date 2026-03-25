using ClinicPlatform.Domain.Entities;
using ClinicPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicPlatform.Infrastructure.Persistence.Configurations;

public class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.ToTable("visits");

        builder.Property(x => x.Id).HasDefaultValueSql("(UUID())");
        builder.Property(x => x.QueueNumber);
        builder.Property(x => x.CheckinMethod).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.NeedsMedication).HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

        builder.HasIndex(x => new { x.ClinicId, x.Status });
        builder.HasIndex(x => new { x.ClinicId, x.PatientId });
        builder.HasIndex(x => new { x.ClinicId, x.CheckedInAt });

        builder.HasOne(x => x.Patient).WithMany(x => x.Visits)
            .HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Appointment).WithMany()
            .HasForeignKey(x => x.AppointmentId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.WorkflowDefinition).WithMany(x => x.Visits)
            .HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CurrentStep).WithMany()
            .HasForeignKey(x => x.CurrentStepId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Doctor).WithMany()
            .HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Room).WithMany()
            .HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Clinic).WithMany()
            .HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Restrict);
    }
}
