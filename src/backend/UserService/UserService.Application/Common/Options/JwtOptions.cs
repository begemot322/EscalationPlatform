namespace UserService.Application.Common.Options;

public class JwtOptions
{
    public const string SectionName = "JwtOptions";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiresHours { get; set; } = 1;
}
