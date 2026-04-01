namespace ClinicPlatform.Infrastructure.Services.AI;

public static class CommandPromptBuilder
{
    private static readonly Dictionary<string, string[]> RolePermissions = new()
    {
        ["Nurse"] = ["call_next", "skip", "query_queue", "query_stats"],
        ["Doctor"] = ["complete_consult", "create_prescription", "query_queue", "query_stats"],
        ["Admin"] = ["query_queue", "query_stats"],
    };

    public static string BuildSystemPrompt(string role)
    {
        var allowed = RolePermissions.GetValueOrDefault(role, ["query_stats"]);
        var actionList = string.Join("\n", allowed.Select(a => $"- {a}：{GetActionDescription(a)}"));

        var paramsDoc = """
            各 action 的 params 格式：
            - call_next：{"queueType": "waiting"}
            - skip：{"queueNumber": 號碼數字}
            - query_queue：{}
            - complete_consult：{"needsMedication": true或false}
            - create_prescription：{"drugName": "藥品名稱", "dosage": "劑量如500mg", "frequency": "QD/BID/TID/QID", "days": 天數}
            - query_stats：{}
            """;

        return $"""
            你是醫療院所的智慧指令助手。根據使用者的自然語言指令，解析意圖並回傳對應的 action。
            目前使用者角色：{role}

            此角色可執行的 action：
            {actionList}

            {paramsDoc}

            你的回覆必須是且僅是一個合法的 JSON 物件，包含以下欄位：
            action（字串）、params（物件）、message（繁體中文字串）、needsConfirm（布林值，操作類 true、查詢類 false）

            禁止輸出 JSON 以外的任何文字、說明或換行。
            """;
    }

    public static string BuildUserPrompt(string command)
    {
        return $"使用者指令：{command}";
    }

    private static string GetActionDescription(string action) => action switch
    {
        "call_next" => "叫下一位候診病患",
        "skip" => "跳過指定號碼的病患（過號）",
        "query_queue" => "查詢目前候診人數與狀態",
        "complete_consult" => "完成看診，可選擇是否需要拿藥",
        "create_prescription" => "開立處方（藥品名稱、劑量、頻率、天數）",
        "query_stats" => "查詢今日門診統計資料",
        _ => action
    };
}
