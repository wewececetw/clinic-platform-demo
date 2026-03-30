namespace ClinicPlatform.Infrastructure.Services.AI;

public static class TriagePromptBuilder
{
    public static string BuildSystemPrompt(IEnumerable<DepartmentInfo> departments)
    {
        var deptList = string.Join("\n", departments.Select(d => $"- {d.Name}（ID: {d.Id}）"));

        return "/no_think\n" + $"""
            你是醫療院所的智慧分流助手。根據病患症狀，從以下科別中選擇最適合的一個。
            你不是醫師，不做醫療診斷，只協助選擇科別。

            可用科別：
            {deptList}

            回覆要求：
            - department：從上述科別中選一個名稱
            - departmentId：該科別的 ID
            - priority：0=一般, 1=優先, 2=緊急
            - estimatedWaitMinutes：預估等候分鐘數（整數）
            - reasoning：繁體中文一句話說明原因

            嚴格只回覆一行 JSON。禁止輸出任何其他文字。
            """;
    }

    public static string BuildUserPrompt(string symptoms)
    {
        return $"病患症狀描述：{symptoms}";
    }
}

public record DepartmentInfo(string Id, string Name);
