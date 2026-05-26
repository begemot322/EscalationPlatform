using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SchedulerService.Models;

namespace SchedulerService.Clients;

/// <summary>
/// Типизированный HTTP-клиент для взаимодействия с внутренними эндпоинтами EscalationService.
/// Retry-политика настраивается при регистрации через AddStandardResilienceHandler.
/// </summary>
public sealed class EscalationServiceClient : IEscalationServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EscalationServiceClient> _logger;

    public EscalationServiceClient(HttpClient httpClient, ILogger<EscalationServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SlaCheckItem>> GetSlaCheckItemsAsync(CancellationToken ct)
    {
        var items = await _httpClient
            .GetFromJsonAsync<List<SlaCheckItem>>("internal/escalation/sla-check", ct);

        if (items is null)
        {
            _logger.LogWarning("EscalationService returned null for sla-check");
            return [];
        }

        return items;
    }

    public async Task EscalatePriorityAsync(long id, CancellationToken ct)
    {
        var response = await _httpClient
            .PatchAsync($"internal/escalation/{id}/escalate-priority", null, ct);

        if (!response.IsSuccessStatusCode)
            _logger.LogWarning(
                "Failed to escalate priority for escalation {Id}: {Status}",
                id, response.StatusCode);
    }
}