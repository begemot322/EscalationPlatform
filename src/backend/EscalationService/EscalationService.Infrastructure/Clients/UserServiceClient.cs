using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using EscalationService.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace EscalationService.Infrastructure.Clients;

/// <summary>
/// HTTP-клиент для взаимодействия с UserService.
/// </summary>
public sealed class UserServiceClient : IUserServiceClient
{
    private readonly HttpClient _http;
    private readonly ILogger<UserServiceClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public UserServiceClient(HttpClient http, ILogger<UserServiceClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<UserLookupResult>> GetByEmailsAsync(
        IEnumerable<string> emails, CancellationToken ct)
        => await PostLookupAsync("internal/user/by-emails", emails, ct);

    public async Task<List<UserLookupResult>> GetByIdsAsync(
        IEnumerable<Guid> ids, CancellationToken ct)
        => await PostLookupAsync("internal/user/by-ids", ids, ct);

    private async Task<List<UserLookupResult>> PostLookupAsync<T>(
        string endpoint, IEnumerable<T> payload, CancellationToken ct)
    {
        var list = payload.ToList();
        if (list.Count == 0) return [];

        try
        {
            var json = JsonSerializer.Serialize(list);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(endpoint, content, ct);
            response.EnsureSuccessStatusCode();

            var users = await response.Content
                .ReadFromJsonAsync<List<UserResponse>>(JsonOptions, ct) ?? [];

            return users.Select(u => new UserLookupResult(u.Id, u.Email, u.FirstName)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call {Endpoint} from UserService", endpoint);
            return [];
        }
    }

    // Внутренняя модель для десериализации ответа UserService
    private sealed record UserResponse(Guid Id, string Email, string FirstName);
}
