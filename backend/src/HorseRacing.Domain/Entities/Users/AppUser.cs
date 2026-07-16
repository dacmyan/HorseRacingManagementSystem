namespace HorseRacing.Domain.Entities;

public class AppUser
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public Role? Role { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? VerificationToken { get; set; }
    public bool IsEmailConfirmed { get; set; } = false;
    public DateTime? TokenExpiresAt { get; set; }
    public ICollection<Horse> Horses { get; set; }
    = new List<Horse>();
}
