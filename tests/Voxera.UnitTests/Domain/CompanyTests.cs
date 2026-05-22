using FluentAssertions;
using Voxera.Domain.Entities;
using Voxera.Domain.Enums;
using Xunit;

namespace Voxera.UnitTests.Domain;

public class CompanyTests
{
    [Fact]
    public void Create_ShouldSetCorrectDefaults()
    {
        // Arrange & Act
        var company = Company.Create("Test Company", "test-company");

        // Assert
        company.Name.Should().Be("Test Company");
        company.Slug.Should().Be("test-company");
        company.Status.Should().Be(CompanyStatus.Active);
        company.Plan.Should().Be(SubscriptionPlan.Starter);
        company.IsTrialActive.Should().BeTrue();
        company.TrialEndsAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(14), TimeSpan.FromMinutes(1));
        company.MaxExtensions.Should().Be(10);
        company.MaxConcurrentCalls.Should().Be(5);
        company.SipDomain.Should().Be("test-company.sip.voxera.io");
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        // Act
        var company = Company.Create("Test Company", "test-company");

        // Assert
        company.DomainEvents.Should().HaveCount(1);
        company.DomainEvents.First().Should().BeOfType<CompanyCreatedEvent>();
        var evt = (CompanyCreatedEvent)company.DomainEvents.First();
        evt.Name.Should().Be("Test Company");
    }

    [Fact]
    public void UpdatePlan_Business_ShouldSetCorrectLimits()
    {
        // Arrange
        var company = Company.Create("Test", "test");

        // Act
        company.UpdatePlan(SubscriptionPlan.Business, DateTime.UtcNow.AddMonths(1));

        // Assert
        company.Plan.Should().Be(SubscriptionPlan.Business);
        company.MaxExtensions.Should().Be(50);
        company.MaxConcurrentCalls.Should().Be(25);
        company.IsTrialActive.Should().BeFalse();
    }

    [Fact]
    public void Suspend_ShouldChangStatusToSuspended()
    {
        // Arrange
        var company = Company.Create("Test", "test");

        // Act
        company.Suspend();

        // Assert
        company.Status.Should().Be(CompanyStatus.Suspended);
    }

    [Fact]
    public void SoftDelete_ShouldMarkAsDeleted()
    {
        // Arrange
        var company = Company.Create("Test", "test");

        // Act
        company.SoftDelete();

        // Assert
        company.IsDeleted.Should().BeTrue();
        company.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
