using System.Net;
using System.Net.Http.Json;
using PlantMonitoringAPI.DTOs;
using PlantMonitoringAPI.Models;
using Xunit;

namespace PlantMonitoringAPI.Tests.Controllers;

public class NotificationsControllerTests : IntegrationTestBase
{
    public NotificationsControllerTests(PlantMonitoringWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetNotifications_ReturnsAll_OrderedNewestFirst()
    {
        WithDb(db =>
        {
            var now = DateTime.UtcNow;
            db.Notifications.Add(new Notification
            {
                Title = "First", Message = "msg",
                Severity = "warning",
                CreatedAt = now.AddMinutes(-10)
            });
            db.Notifications.Add(new Notification
            {
                Title = "Second", Message = "msg",
                Severity = "warning",
                CreatedAt = now.AddMinutes(-2)
            });
            db.SaveChanges();
        });

        var response = await Client.GetAsync("/api/notifications");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<NotificationDto>>();

        Assert.NotNull(list);
        Assert.Equal(2, list!.Count);
        Assert.Equal("Second", list[0].Title);
        Assert.Equal("First", list[1].Title);
    }

    [Fact]
    public async Task GetNotifications_UnreadOnly_FiltersCorrectly()
    {
        WithDb(db =>
        {
            db.Notifications.Add(new Notification
            {
                Title = "Read item", Message = "msg",
                Severity = "warning", IsRead = true
            });
            db.Notifications.Add(new Notification
            {
                Title = "Unread item", Message = "msg",
                Severity = "warning", IsRead = false
            });
            db.SaveChanges();
        });

        var response = await Client.GetAsync("/api/notifications?unreadOnly=true");
        var list = await response.Content.ReadFromJsonAsync<List<NotificationDto>>();
        var only = Assert.Single(list!);
        Assert.Equal("Unread item", only.Title);
    }

    [Fact]
    public async Task MarkAsRead_FlipsFlag()
    {
        int notificationId = 0;
        WithDb(db =>
        {
            var n = new Notification
            {
                Title = "Plant is dry", Message = "msg",
                Severity = "warning", IsRead = false
            };
            db.Notifications.Add(n);
            db.SaveChanges();
            notificationId = n.Id;
        });

        var response = await Client.PatchAsync($"/api/notifications/{notificationId}/read", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        WithDb(db =>
        {
            var stored = db.Notifications.Find(notificationId);
            Assert.NotNull(stored);
            Assert.True(stored!.IsRead);
        });
    }

    [Fact]
    public async Task MarkAsRead_Returns404_ForUnknownId()
    {
        var response = await Client.PatchAsync("/api/notifications/9999/read", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteNotification_Returns204AndRemovesRow()
    {
        int notificationId = 0;
        WithDb(db =>
        {
            var n = new Notification
            {
                Title = "Delete me", Message = "msg",
                Severity = "warning"
            };
            db.Notifications.Add(n);
            db.SaveChanges();
            notificationId = n.Id;
        });

        var response = await Client.DeleteAsync($"/api/notifications/{notificationId}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        WithDb(db =>
        {
            Assert.Null(db.Notifications.Find(notificationId));
        });
    }
}
