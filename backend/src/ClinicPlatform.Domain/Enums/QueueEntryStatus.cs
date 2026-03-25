namespace ClinicPlatform.Domain.Enums;

public enum QueueEntryStatus
{
    Waiting = 1,
    Called = 2,
    InProgress = 3,
    Completed = 4,
    Skipped = 5,
    NoShow = 6
}
