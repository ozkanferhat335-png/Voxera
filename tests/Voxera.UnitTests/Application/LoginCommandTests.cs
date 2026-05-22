using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Voxera.Application.Commands.Auth;
using Voxera.Application.Interfaces;
using Voxera.Domain.Entities;
using Voxera.Domain.Enums;
using Voxera.Infrastructure.Persistence;
using Xunit;

namespace Voxera.UnitTests.Application;

public class LoginCommandTests
{
    private ApplicationDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var db = CreateInMemoryDb();
        var company = Company.Create("Test Corp", "test-corp");
        await db.Companies.AddAsync(company);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!", 4);
        var user = User.Create(company.Id, "John", "Doe", "john@test.com", passwordHash, UserRole.CompanyAdmin);
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();

        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("access-token");
        tokenService.Setup(t => t.GenerateRefreshToken()).Returns("refresh-token");

        var cache = new Mock<ICacheService>();
        var logger = new Mock<ILogger<LoginCommandHandler>>();

        var handler = new LoginCommandHandler(db, tokenService.Object, cache.Object, logger.Object);
        var command = new LoginCommand("john@test.com", "Password123!", "127.0.0.1", "TestAgent");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.Email.Should().Be("john@test.com");
        result.Role.Should().Be("CompanyAdmin");
    }

    [Fact]
    public async Task Handle_InvalidPassword_ShouldThrowUnauthorized()
    {
        // Arrange
        var db = CreateInMemoryDb();
        var company = Company.Create("Test Corp", "test-corp");
        await db.Companies.AddAsync(company);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword", 4);
        var user = User.Create(company.Id, "John", "Doe", "john@test.com", passwordHash);
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();

        var tokenService = new Mock<ITokenService>();
        var cache = new Mock<ICacheService>();
        var logger = new Mock<ILogger<LoginCommandHandler>>();

        var handler = new LoginCommandHandler(db, tokenService.Object, cache.Object, logger.Object);
        var command = new LoginCommand("john@test.com", "WrongPassword", "127.0.0.1", null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistentUser_ShouldThrowUnauthorized()
    {
        // Arrange
        var db = CreateInMemoryDb();
        var tokenService = new Mock<ITokenService>();
        var cache = new Mock<ICacheService>();
        var logger = new Mock<ILogger<LoginCommandHandler>>();

        var handler = new LoginCommandHandler(db, tokenService.Object, cache.Object, logger.Object);
        var command = new LoginCommand("nonexistent@test.com", "Password123!", "127.0.0.1", null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }
}
