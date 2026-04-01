namespace ClinicPlatform.Domain.Enums;

public enum CommandAction
{
    CallNext,
    Skip,
    QueryQueue,
    CompleteConsult,
    CreatePrescription,
    QueryStats,
    Unknown
}
