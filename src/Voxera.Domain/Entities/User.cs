using Voxera.Domain.Enums;

namespace Voxera.Domain.Entities;

public class User : BaseEntity
{
    public Guid CompanyId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public UserRole Role { get; private set; } = UserRole.Operator;
    public UserStatus Status { get; private set; } = UserStatus.Active;
    public string? AvatarUrl { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiresAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public string? LastLoginIp { get; private set; }
    public bool TwoFactorEnabled { get; private set; } = false;
    public string? TwoFactorSecret { get; private set; }
    public int FailedLoginAttempts { get; private set; } = 0;
    public DateTime? LockedUntil { get; private set; }
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }
    public string? TimeZone { get; private set; } = "Europe/Istanbul";
    public string? Language { get; private set; } = "tr";

    // Navigation
    public Company? Company { get; private set; }
    public Extension? Extension { get; private set; }
    public ICollection<AuditLog> AuditLogs { get; private set; } = new List<AuditLog>();

    public string FullName => $"{FirstName} {LastName}";
    public bool IsLocked => LockedUntil.HasValue && LockedUntil > DateTime.UtcNow;

    protected User() { }

    public static User Create(Guid companyId, string firstName, string lastName, string email, string passwordHash, UserRole role = UserRole.Operator)
    {
        var user = new User
        {
            CompanyId = companyId,
            FirstName = firstName,
            LastName = lastName,
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role
        };
        user.AddDomainEvent(new UserCreatedEvent(user.Id, companyId, email, role));
        return user;
    }

    public void SetRefreshToken(string token, DateTime expiresAt)
    {
        RefreshToken = token;
        RefreshTokenExpiresAt = expiresAt;
    }

    public void RecordLogin(string ipAddress)
    {
        LastLoginAt = DateTime.UtcNow;
        LastLoginIp = ipAddress;
        FailedLoginAttempts = 0;
        LockedUntil = null;
        MarkAsUpdated();
    }

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= 5)
            LockedUntil = DateTime.UtcNow.AddMinutes(30);
        MarkAsUpdated();
    }

    public void SetPasswordResetToken(string token)
    {
        PasswordResetToken = token;
        PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
        MarkAsUpdated();
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;
        MarkAsUpdated();
    }

    public void UpdateProfile(string firstName, string lastName, string? phone)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phone;
        MarkAsUpdated();
    }

    public void ChangeRole(UserRole role) { Role = role; MarkAsUpdated(); }
    public void Deactivate() { Status = UserStatus.Inactive; MarkAsUpdated(); }
    public void Activate() { Status = UserStatus.Active; MarkAsUpdated(); }
}

public record UserCreatedEvent(Guid UserId, Guid CompanyId, string Email, UserRole Role) : DomainEvent;
