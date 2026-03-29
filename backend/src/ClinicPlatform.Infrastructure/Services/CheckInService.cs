using ClinicPlatform.Application.Common;
using ClinicPlatform.Application.Features.CheckIn;
using ClinicPlatform.Domain.Entities;
using ClinicPlatform.Domain.Enums;
using ClinicPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicPlatform.Infrastructure.Services;

public class CheckInService(ClinicDbContext db) : ICheckInService
{
    public async Task<Result> SendOtpAsync(SendOtpRequest request)
    {
        var otp = new OtpVerification
        {
            Id = Guid.NewGuid(),
            ClinicId = request.ClinicId,
            Phone = request.Phone,
            OtpCode = Random.Shared.Next(100000, 999999).ToString(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow
        };

        db.OtpVerifications.Add(otp);
        await db.SaveChangesAsync();

        // TODO: 透過 SMS 或其他管道發送 OTP
        return Result.Ok();
    }

    public async Task<Result<CheckInResponse>> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var otp = await db.OtpVerifications
            .Where(o => o.ClinicId == request.ClinicId
                && o.Phone == request.Phone
                && o.OtpCode == request.OtpCode
                && !o.IsUsed
                && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (otp is null)
            return Result<CheckInResponse>.Fail("OTP 驗證失敗或已過期");

        otp.IsUsed = true;

        var patient = await FindOrCreatePatientAsync(request.ClinicId, request.Phone, null);
        var (visit, queueEntry) = await CreateVisitAsync(patient, CheckinMethod.Otp);

        await db.SaveChangesAsync();

        return Result<CheckInResponse>.Ok(new CheckInResponse(
            visit.Id, queueEntry.QueueNumber, visit.CurrentStep!.StepCode));
    }

    public async Task<Result<CheckInResponse>> QrCodeCheckInAsync(QrCodeCheckInRequest request)
    {
        var appointment = await db.Appointments
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.ClinicId == request.ClinicId
                && a.QrCodeToken == request.QrCodeToken
                && a.Status == AppointmentStatus.Booked);

        if (appointment is null)
            return Result<CheckInResponse>.Fail("無效的 QR Code 或預約已報到");

        appointment.Status = AppointmentStatus.CheckedIn;

        var (visit, queueEntry) = await CreateVisitAsync(
            appointment.Patient, CheckinMethod.QrCode, appointment.Id, appointment.DoctorId);

        await db.SaveChangesAsync();

        return Result<CheckInResponse>.Ok(new CheckInResponse(
            visit.Id, queueEntry.QueueNumber, visit.CurrentStep!.StepCode));
    }

    public async Task<Result<CheckInResponse>> ManualCheckInAsync(ManualCheckInRequest request, Guid nurseUserId)
    {
        Patient patient;

        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            patient = await FindOrCreatePatientAsync(request.ClinicId, request.Phone, request.FullName);
        }
        else
        {
            patient = new Patient
            {
                Id = Guid.NewGuid(),
                ClinicId = request.ClinicId,
                FullName = request.FullName,
                IsAnonymous = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Patients.Add(patient);
        }

        var (visit, queueEntry) = await CreateVisitAsync(
            patient, CheckinMethod.Manual, doctorId: request.DoctorId);

        await db.SaveChangesAsync();

        return Result<CheckInResponse>.Ok(new CheckInResponse(
            visit.Id, queueEntry.QueueNumber, visit.CurrentStep!.StepCode));
    }

    private async Task<Patient> FindOrCreatePatientAsync(Guid clinicId, string phone, string? fullName)
    {
        var patient = await db.Patients
            .FirstOrDefaultAsync(p => p.ClinicId == clinicId && p.Phone == phone);

        if (patient is not null)
        {
            if (fullName is not null && patient.FullName != fullName)
            {
                patient.FullName = fullName;
                patient.UpdatedAt = DateTime.UtcNow;
            }
            return patient;
        }

        patient = new Patient
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            Phone = phone,
            FullName = fullName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Patients.Add(patient);
        return patient;
    }

    private async Task<(Visit visit, QueueEntry queueEntry)> CreateVisitAsync(
        Patient patient,
        CheckinMethod method,
        Guid? appointmentId = null,
        Guid? doctorId = null)
    {
        var workflow = await db.WorkflowDefinitions
            .Where(w => w.ClinicId == patient.ClinicId && w.IsDefault)
            .FirstAsync();

        var firstStep = await db.WorkflowSteps
            .Where(s => s.WorkflowDefinitionId == workflow.Id)
            .OrderBy(s => s.StepOrder)
            .FirstAsync();

        var queueNumber = await GetNextQueueNumberAsync(patient.ClinicId);

        var visit = new Visit
        {
            Id = Guid.NewGuid(),
            ClinicId = patient.ClinicId,
            PatientId = patient.Id,
            AppointmentId = appointmentId,
            WorkflowDefinitionId = workflow.Id,
            CurrentStepId = firstStep.Id,
            DoctorId = doctorId,
            QueueNumber = queueNumber,
            CheckinMethod = method,
            Status = VisitStatus.Active,
            CheckedInAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        visit.CurrentStep = firstStep;
        db.Visits.Add(visit);

        var queueEntry = new QueueEntry
        {
            Id = Guid.NewGuid(),
            ClinicId = patient.ClinicId,
            VisitId = visit.Id,
            QueueType = QueueType.Consulting,
            QueueNumber = queueNumber,
            Status = QueueEntryStatus.Waiting,
            CreatedAt = DateTime.UtcNow
        };
        db.QueueEntries.Add(queueEntry);

        var visitEvent = new VisitEvent
        {
            Id = Guid.NewGuid(),
            ClinicId = patient.ClinicId,
            VisitId = visit.Id,
            ToStepId = firstStep.Id,
            TriggerType = TriggerType.System,
            CreatedAt = DateTime.UtcNow
        };
        db.VisitEvents.Add(visitEvent);

        return (visit, queueEntry);
    }

    private async Task<int> GetNextQueueNumberAsync(Guid clinicId)
    {
        var today = DateTime.UtcNow.Date;
        var maxNumber = await db.Visits
            .Where(v => v.ClinicId == clinicId && v.CheckedInAt >= today)
            .MaxAsync(v => (int?)v.QueueNumber) ?? 0;

        return maxNumber + 1;
    }
}
