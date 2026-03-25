using ClinicPlatform.Application.Common;

namespace ClinicPlatform.Application.Features.Prescription;

public interface IPrescriptionService
{
    Task<Result<PrescriptionDto>> CreateAsync(CreatePrescriptionRequest request, Guid doctorUserId);
    Task<Result<List<PrescriptionDto>>> GetPharmacyQueueAsync(Guid clinicId);
    Task<Result> StartDispenseAsync(Guid clinicId, Guid prescriptionId, Guid pharmacistUserId);
    Task<Result> CompleteDispenseAsync(Guid clinicId, Guid prescriptionId, Guid pharmacistUserId);
}
