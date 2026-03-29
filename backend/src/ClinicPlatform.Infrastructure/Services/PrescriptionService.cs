using ClinicPlatform.Application.Common;
using ClinicPlatform.Application.Features.Prescription;
using ClinicPlatform.Application.Features.Workflow;
using ClinicPlatform.Domain.Entities;
using ClinicPlatform.Domain.Enums;
using ClinicPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicPlatform.Infrastructure.Services;

public class PrescriptionService(ClinicDbContext db, IWorkflowEngine workflowEngine) : IPrescriptionService
{
    public async Task<Result<PrescriptionDto>> CreateAsync(CreatePrescriptionRequest request, Guid doctorUserId)
    {
        var visit = await db.Visits
            .FirstOrDefaultAsync(v => v.ClinicId == request.ClinicId && v.Id == request.VisitId);

        if (visit is null)
            return Result<PrescriptionDto>.Fail("找不到該就診紀錄");

        var prescription = new Prescription
        {
            Id = Guid.NewGuid(),
            ClinicId = request.ClinicId,
            VisitId = request.VisitId,
            DoctorId = doctorUserId,
            Status = PrescriptionStatus.Draft,
            Notes = request.Notes,
            PrescribedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in request.Items)
        {
            prescription.Items.Add(new PrescriptionItem
            {
                Id = Guid.NewGuid(),
                ClinicId = request.ClinicId,
                PrescriptionId = prescription.Id,
                MedicationId = item.MedicationId,
                Dosage = item.Dosage,
                Frequency = item.Frequency,
                DurationDays = item.DurationDays,
                Quantity = item.Quantity,
                Instructions = item.Instructions
            });
        }

        db.Prescriptions.Add(prescription);
        await db.SaveChangesAsync();

        // 推進工作流程
        await workflowEngine.AdvanceAsync(request.ClinicId, request.VisitId, doctorUserId);

        var doctor = await db.Users.FindAsync(doctorUserId);

        return Result<PrescriptionDto>.Ok(new PrescriptionDto(
            prescription.Id,
            prescription.VisitId,
            prescription.Status.ToString(),
            doctor?.DisplayName ?? "未知醫師",
            prescription.Items.Select(i => new PrescriptionItemDto(
                "", // 藥品名稱需另查，此處暫以空字串代替
                i.Dosage,
                i.Frequency,
                i.DurationDays,
                i.Quantity,
                i.Instructions)).ToList(),
            prescription.PrescribedAt));
    }

    public async Task<Result<List<PrescriptionDto>>> GetPharmacyQueueAsync(Guid clinicId)
    {
        var prescriptions = await db.Prescriptions
            .Include(p => p.Items)
                .ThenInclude(i => i.Medication)
            .Include(p => p.Doctor)
            .Include(p => p.Visit)
                .ThenInclude(v => v.Patient)
            .Where(p => p.ClinicId == clinicId && p.Status == PrescriptionStatus.Sent)
            .OrderBy(p => p.SentToPharmacyAt)
            .ToListAsync();

        var result = prescriptions.Select(p => new PrescriptionDto(
            p.Id,
            p.VisitId,
            p.Status.ToString(),
            p.Doctor.DisplayName ?? "未知醫師",
            p.Items.Select(i => new PrescriptionItemDto(
                i.Medication.Name,
                i.Dosage,
                i.Frequency,
                i.DurationDays,
                i.Quantity,
                i.Instructions)).ToList(),
            p.PrescribedAt)).ToList();

        return Result<List<PrescriptionDto>>.Ok(result);
    }

    public async Task<Result> StartDispenseAsync(Guid clinicId, Guid prescriptionId, Guid pharmacistUserId)
    {
        var prescription = await db.Prescriptions
            .FirstOrDefaultAsync(p => p.ClinicId == clinicId && p.Id == prescriptionId);

        if (prescription is null)
            return Result.Fail("找不到該處方");

        if (prescription.Status != PrescriptionStatus.Sent)
            return Result.Fail("處方狀態不正確，無法開始配藥");

        prescription.Status = PrescriptionStatus.Dispensing;

        await db.SaveChangesAsync();

        return await workflowEngine.AdvanceAsync(clinicId, prescription.VisitId, pharmacistUserId);
    }

    public async Task<Result> CompleteDispenseAsync(Guid clinicId, Guid prescriptionId, Guid pharmacistUserId)
    {
        var prescription = await db.Prescriptions
            .FirstOrDefaultAsync(p => p.ClinicId == clinicId && p.Id == prescriptionId);

        if (prescription is null)
            return Result.Fail("找不到該處方");

        if (prescription.Status != PrescriptionStatus.Dispensing)
            return Result.Fail("處方狀態不正確，無法完成配藥");

        prescription.Status = PrescriptionStatus.Dispensed;
        prescription.DispensedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return await workflowEngine.AdvanceAsync(clinicId, prescription.VisitId, pharmacistUserId);
    }
}
