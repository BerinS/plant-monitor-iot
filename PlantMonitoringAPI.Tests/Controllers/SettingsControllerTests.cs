using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PlantMonitoringAPI.DTOs;
using PlantMonitoringAPI.Models;
using PlantMonitoringAPI.Services;
using Xunit;

namespace PlantMonitoringAPI.Tests.Controllers;

public class SettingsControllerTests : IntegrationTestBase
{
    public SettingsControllerTests(PlantMonitoringWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_ReturnsSeededSettings()
    {
        WithDb(db =>
        {
            db.SystemSettings.Add(new SystemSetting { Key = SettingKeys.MailEnabled, Value = "false" });
            db.SystemSettings.Add(new SystemSetting { Key = SettingKeys.MailHost, Value = "smtp.example.com" });
            db.SaveChanges();
        });

        var response = await Client.GetAsync("/api/settings");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var settings = await response.Content.ReadFromJsonAsync<List<SettingDto>>();

        Assert.NotNull(settings);
        Assert.Equal(2, settings!.Count);
        Assert.Contains(settings, s => s.Key == SettingKeys.MailHost && s.Value == "smtp.example.com");
    }

    [Fact]
    public async Task UpdateBulk_PersistsValues_AndReloadsCache()
    {
        WithDb(db =>
        {
            db.SystemSettings.Add(new SystemSetting { Key = SettingKeys.MailEnabled, Value = "false" });
            db.SystemSettings.Add(new SystemSetting { Key = SettingKeys.MailHost, Value = "old" });
            db.SaveChanges();
        });

        // Prime the cache through ISettingsService so the test verifies a real reload,
        // not just a first-time load that happened to read the new values.
        var settingsService = Factory.Services.GetRequiredService<ISettingsService>();
        Assert.Equal("false", await settingsService.GetAsync(SettingKeys.MailEnabled));
        Assert.Equal("old", await settingsService.GetAsync(SettingKeys.MailHost));

        var update = new UpdateSettingsBulkDto
        {
            Settings = new Dictionary<string, string?>
            {
                [SettingKeys.MailEnabled] = "true",
                [SettingKeys.MailHost] = "smtp.new.example.com"
            }
        };

        var response = await Client.PutAsJsonAsync("/api/settings", update);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Persistence: the controller's GET reads directly from the database.
        var follow = await Client.GetAsync("/api/settings");
        var settings = await follow.Content.ReadFromJsonAsync<List<SettingDto>>();
        Assert.Equal("true", settings!.Single(s => s.Key == SettingKeys.MailEnabled).Value);
        Assert.Equal("smtp.new.example.com", settings.Single(s => s.Key == SettingKeys.MailHost).Value);

        // Cache reload: ISettingsService is consulted by background services (email loop)
        // and the test-email endpoint. After SetManyAsync, the in-memory cache must reflect
        // the new values — if it did not, the email loop would keep using stale credentials.
        Assert.Equal("true", await settingsService.GetAsync(SettingKeys.MailEnabled));
        Assert.Equal("smtp.new.example.com", await settingsService.GetAsync(SettingKeys.MailHost));
    }

    [Fact]
    public async Task UpdateBulk_UnknownKey_Returns400()
    {
        var update = new UpdateSettingsBulkDto
        {
            Settings = new Dictionary<string, string?>
            {
                ["definitely_not_a_real_setting"] = "x"
            }
        };

        var response = await Client.PutAsJsonAsync("/api/settings", update);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
