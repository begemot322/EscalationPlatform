using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SchedulerService.Models;

namespace SchedulerService.Clients;

/// <summary>
/// HTTP-клиент для взаимодействия с UserService.
/// </summary>
public sealed class UserServiceClient : IUserServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserServiceClient> _logger;

    public UserServiceClient(HttpClient httpClient, ILogger<UserServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UserProfile?> GetUserProfileAsync(Guid userId, CancellationToken ct)
    {
        var response = await _httpClient
            .GetAsync($"internal/user/{userId}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("User {UserId} not found in UserService", userId);
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<UserProfile>(ct);
    }
}
