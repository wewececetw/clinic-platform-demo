namespace ClinicPlatform.WebAPI.Hubs;

/// <summary>
/// 集中定義 SignalR 群組命名規則，避免 Hub 與 Publisher 兩邊字串不一致
/// </summary>
public static class GroupNames
{
    public static string ClinicGroup(Guid clinicId) => $"clinic_{clinicId}";

    public static string VisitGroup(Guid visitId) => $"visit_{visitId}";
}
