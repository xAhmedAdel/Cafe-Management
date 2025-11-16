using Xunit;
using CafeManagement.Core.Entities;
using CafeManagement.Core.Enums;

namespace CafeManagement.Tests.Core;

public class EntityTests
{
    [Fact]
    public void User_CreatedWithDefaults_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        Assert.Equal(0, user.Id);
        Assert.Equal(0.00m, user.Balance);
        Assert.Equal(UserRole.Customer, user.Role);
    }

    [Fact]
    public void Client_CreatedWithDefaults_ShouldHaveCorrectStatus()
    {
        // Arrange & Act
        var client = new Client();

        // Assert
        Assert.Equal(ClientStatus.Offline, client.Status);
        Assert.False(client.LastSeen.HasValue);
        Assert.Null(client.CurrentSessionId);
    }

    [Fact]
    public void Session_CreatedWithDefaults_ShouldBeActive()
    {
        // Arrange & Act
        var session = new Session();

        // Assert
        Assert.Equal(SessionStatus.Active, session.Status);
        Assert.Equal(0.00m, session.TotalAmount);
        Assert.Equal(2.00m, session.HourlyRate);
    }

    [Fact]
    public void LockScreenConfig_CreatedWithDefaults_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var lockConfig = new LockScreenConfig();

        // Assert
        Assert.Equal("#000000", lockConfig.BackgroundColor);
        Assert.Equal("#FFFFFF", lockConfig.TextColor);
        Assert.True(lockConfig.ShowTimeRemaining);
        Assert.Empty(lockConfig.Message);
    }

    [Fact]
    public void UsageLog_CreatedWithDefaults_ShouldHaveCurrentTimestamp()
    {
        // Arrange & Act
        var usageLog = new UsageLog();

        // Assert
        Assert.NotNull(usageLog.Timestamp);
        Assert.True(usageLog.Timestamp > DateTime.UtcNow.AddMinutes(-1));
    }
}