namespace UserService.Domain;

public class OtpCode
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsUsed { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}