namespace EscalationPlatform.Shared.Options;

public class JwtOptions
{
    public const string SectionName = "JwtOptions";

    public string SecretKey { get; init; } = null!;
    public string? Issuer { get; init; }
    public string? Audience { get; init; }
    public int ExpiresHours { get; init; } = 24;
}
