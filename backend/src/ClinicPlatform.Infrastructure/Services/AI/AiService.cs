using System.Text.Json;
using ClinicPlatform.Application.Common;
using ClinicPlatform.Application.Features.AI;
using ClinicPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ClinicPlatform.Infrastructure.Services.AI;

public class AiService(
    IEnumerable<ILlmClient> llmClients,
    ClinicDbContext db,
    IConfiguration configuration,
    ILogger<AiService> logger) : IAiService
{
    public async Task<Result<TriageResponse>> TriageAsync(TriageRequest request)
    {
        // 取得該院所的科別清單
        var departments = await db.Departments
            .Where(d => d.ClinicId == request.ClinicId && d.IsActive)
            .Select(d => new DepartmentInfo(d.Id.ToString(), d.Name))
            .ToListAsync();

        if (departments.Count == 0)
            return Result<TriageResponse>.Fail("該院所尚無可用科別");

        var systemPrompt = TriagePromptBuilder.BuildSystemPrompt(departments);
        var userPrompt = TriagePromptBuilder.BuildUserPrompt(request.Symptoms);

        var llmRequest = new LlmRequest(
            Model: null!,
            Messages:
            [
                new LlmMessage("system", systemPrompt),
                new LlmMessage("user", userPrompt)
            ],
            Temperature: 0.2f,
            MaxTokens: 2048);

        // 依優先序嘗試各 LLM provider
        var preferredProvider = configuration["AI:Provider"] ?? "Omlx";
        var orderedClients = llmClients
            .OrderByDescending(c => c.ProviderName == preferredProvider)
            .ToList();

        foreach (var client in orderedClients)
        {
            try
            {
                logger.LogInformation("嘗試使用 {Provider} 進行 AI 分流", client.ProviderName);
                var response = await client.ChatAsync(llmRequest);
                logger.LogDebug("LLM 原始回應：{Content}", response.Content[..Math.Min(500, response.Content.Length)]);
                var triage = ParseTriageResponse(response.Content, departments);
                if (triage is not null)
                    return Result<TriageResponse>.Ok(triage);

                logger.LogWarning("{Provider} 回應無法解析，嘗試下一個 provider", client.ProviderName);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "{Provider} 呼叫失敗，嘗試下一個 provider", client.ProviderName);
            }
        }

        return Result<TriageResponse>.Fail("AI 分流服務暫時無法使用，請手動選擇科別");
    }

    public Task<Result<CommandResponse>> CommandAsync(CommandRequest request)
    {
        // Phase 2 實作
        return Task.FromResult(Result<CommandResponse>.Fail("自然語言指令功能開發中"));
    }

    private static TriageResponse? ParseTriageResponse(string content, List<DepartmentInfo> departments)
    {
        try
        {
            // 從回應中提取包含 "department" 的 JSON（跳過 thinking process）
            var json = ExtractJson(content);
            if (json is null) return null;

            var parsed = JsonSerializer.Deserialize<JsonElement>(json);

            var department = parsed.GetProperty("department").GetString() ?? "";
            var reasoning = parsed.GetProperty("reasoning").GetString() ?? "";
            var priority = parsed.TryGetProperty("priority", out var p) ? p.GetInt32() : 0;
            var wait = parsed.TryGetProperty("estimatedWaitMinutes", out var w) ? w.GetInt32() : 15;

            // 嘗試匹配科別 ID
            Guid? deptId = null;
            if (parsed.TryGetProperty("departmentId", out var dId) &&
                Guid.TryParse(dId.GetString(), out var parsedId))
            {
                deptId = parsedId;
            }
            else
            {
                var match = departments.FirstOrDefault(d => d.Name == department);
                if (match is not null) deptId = Guid.Parse(match.Id);
            }

            return new TriageResponse(department, deptId, priority, wait, reasoning);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 從 LLM 回應中提取含 "department" 的 JSON 物件
    /// 處理 Qwen thinking process 等額外文字
    /// </summary>
    private static string? ExtractJson(string content)
    {
        // 從後往前找，因為 thinking 內容在前面
        var idx = content.Length - 1;
        while (idx >= 0)
        {
            var end = content.LastIndexOf('}', idx);
            if (end < 0) break;

            // 找到對應的 {
            var depth = 0;
            for (var i = end; i >= 0; i--)
            {
                if (content[i] == '}') depth++;
                else if (content[i] == '{') depth--;

                if (depth == 0)
                {
                    var candidate = content[i..(end + 1)];
                    if (candidate.Contains("department"))
                        return candidate;
                    break;
                }
            }
            idx = end - 1;
        }
        return null;
    }
}
