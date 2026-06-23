using System.Net;
using System.Net.Http.Json;
using PlantMonitoringAPI.DTOs;
using PlantMonitoringAPI.Models;
using Xunit;

namespace PlantMonitoringAPI.Tests.Controllers;

public class SensorControllerTests : IntegrationTestBase
{
    public SensorControllerTests(PlantMonitoringWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateSensor_ReturnsPlainTokenExactlyOnce()
    {
        var request = new SensorDto
        {
            Name = "Sensor One",
            MacAddress = "AA:BB:CC:DD:EE:01",
            Description = "Living room sensor"
        };

        var response = await Client.PostAsJsonAsync("/api/sensor", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreatedSensorDto>();
        Assert.NotNull(created);
        Assert.False(string.IsNullOrWhiteSpace(created!.PlainApiToken));
        Assert.True(created.Id > 0);

        // Subsequent GET must NOT expose the plain token.
        var listResponse = await Client.GetAsync("/api/sensor");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var raw = await listResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain(created.PlainApiToken, raw);
    }

    [Fact]
    public async Task CreateSensor_DuplicateMac_Returns400()
    {
        WithDb(db =>
        {
            db.Devices.Add(new Device
            {
                Name = "Existing",
                MacAddress = "AA:BB:CC:DD:EE:FF",
                ApiTokenHash = "hash",
                ApiTokenSalt = "salt"
            });
            db.SaveChanges();
        });

        var request = new SensorDto
        {
            Name = "Conflicting",
            MacAddress = "AA:BB:CC:DD:EE:FF"
        };

        var response = await Client.PostAsJsonAsync("/api/sensor", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetHistory_ReturnsReadings_OrderedByMostRecent()
    {
        int plantId = 0;
        WithDb(db =>
        {
            var plant = new Plant { Name = "Charted Plant" };
            db.Plants.Add(plant);
            db.SaveChanges();
            plantId = plant.Id;

            var now = DateTime.UtcNow;
            db.SensorData.Add(new SensorData { PlantId = plantId, MoistureValue = 10, MeasuredAt = now.AddHours(-3) });
            db.SensorData.Add(new SensorData { PlantId = plantId, MoistureValue = 40, MeasuredAt = now.AddHours(-1) });
            db.SensorData.Add(new SensorData { PlantId = plantId, MoistureValue = 25, MeasuredAt = now.AddHours(-2) });
            db.SaveChanges();
        });

        var response = await Client.GetAsync($"/api/sensor/{plantId}/history?hours=24");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var readings = await response.Content.ReadFromJsonAsync<List<SensorHistoryDto>>();
        Assert.NotNull(readings);
        Assert.Equal(3, readings!.Count);
        // Controller sorts descending by MeasuredAt.
        Assert.True(readings[0].Time >= readings[1].Time);
        Assert.True(readings[1].Time >= readings[2].Time);
    }

    [Fact]
    public async Task GetHistory_Returns404_WhenPlantMissing()
    {
        var response = await Client.GetAsync("/api/sensor/9999/history?hours=24");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetLatest_ReturnsMostRecentReading()
    {
        int plantId = 0;
        WithDb(db =>
        {
            var plant = new Plant { Name = "Latest Plant" };
            db.Plants.Add(plant);
            db.SaveChanges();
            plantId = plant.Id;

            var now = DateTime.UtcNow;
            db.SensorData.Add(new SensorData { PlantId = plantId, MoistureValue = 50, MeasuredAt = now.AddMinutes(-30) });
            db.SensorData.Add(new SensorData { PlantId = plantId, MoistureValue = 80, MeasuredAt = now.AddMinutes(-5) });
            db.SaveChanges();
        });

        var response = await Client.GetAsync($"/api/sensor/{plantId}/latest");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var latest = await response.Content.ReadFromJsonAsync<SensorHistoryDto>();
        Assert.NotNull(latest);
        Assert.Equal(80, latest!.Value);
    }
}
