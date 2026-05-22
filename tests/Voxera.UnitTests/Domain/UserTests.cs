using FluentAssertions;
using Voxera.Domain.Entities;
using Voxera.Domain.Enums;
using Xunit;

namespace Voxera.UnitTests.Domain;

public class UserTests
{
    private readonly Guid _companyId = Guid.NewGuid();

    [Fact]
    public void Create_ShouldSetCorrectDefaults()
    {
        // Act
        var user = User.Create(_companyId, "John", "Doe", "john@test.com", "hash", UserRole.Operator);

        // Assert
        user.CompanyId.Should().Be(_companyId);
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.Email.Should().Be("john@test.com");
        user.Role.Should().Be(UserRole.Operator);
        user.Status.Should().Be(UserStatus.Active);
        user.FullName.Should().Be("John Doe");
        user.FailedLoginAttempts.Should().Be(0);
        user.IsLocked.Should().BeFalse();
    }

    [Fact]
    public void RecordFailedLogin_5Times_ShouldLockAccount()
    {
        // Arrange
        var user = User.Create(_companyId, "John", "Doe", "john@test.com", "hash");

        // Act
        for (int i = 0; i < 5; i++)
            user.RecordFailedLogin();

        // Assert
        user.FailedLoginAttempts.Should().Be(5);
        user.IsLocked.Should().BeTrue();
        user.LockedUntil.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(30), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void RecordLogin_ShouldResetFailedAttempts()
    {
        // Arrange
        var user = User.Create(_companyId, "John", "Doe", "john@test.com", "hash");
        user.RecordFailedLogin();
        user.RecordFailedLogin();

        // Act
        user.RecordLogin("192.168.1.1");

        // Assert
        user.FailedLoginAttempts.Should().Be(0);
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.LastLoginIp.Should().Be("192.168.1.1");
        user.IsLocked.Should().BeFalse();
    }

    [Fact]
    public void SetRefreshToken_ShouldUpdateToken()
    {
        // Arrange
        var user = User.Create(_companyId, "John", "Doe", "john@test.com", "hash");
        var expiry = DateTime.UtcNow.AddDays(30);

        // Act
        user.SetRefreshToken("my-refresh-token", expiry);

        // Assert
        user.RefreshToken.Should().Be("my-refresh-token");
        user.RefreshTokenExpiresAt.Should().Be(expiry);
    }

    [Fact]
    public void ChangePassword_ShouldClearResetToken()
    {
        // Arrange
        var user = User.Create(_companyId, "John", "Doe", "john@test.com", "old-hash");
        user.SetPasswordResetToken("reset-token");

        // Act
        user.ChangePassword("new-hash");

        // Assert
        user.PasswordHash.Should().Be("new-hash");
        user.PasswordResetToken.Should().BeNull();
        user.PasswordResetTokenExpiresAt.Should().BeNull();
    }
}
