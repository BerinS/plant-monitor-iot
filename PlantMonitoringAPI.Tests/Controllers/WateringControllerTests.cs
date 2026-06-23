using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PlantMonitoringAPI.DTOs;
using PlantMonitoringAPI.Models;
using Xunit;

namespace PlantMonitoringAPI.Tests.Controllers;

public class WateringControllerTests : IntegrationTestBase
{
    public WateringControllerTests(PlantMonitoringWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task TriggerWatering_HappyPath_Returns200AndPublishesCommand()
    {
        var (plantId, deviceId) = SeedPlantWithDevice();

        var response = await Client.PostAsJsonAsync(
            $"/api/watering/{plantId}",
            new WateringRequestDto { DurationSeconds = 5 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var fake = Factory.GetMqttFake();
        var sent = Assert.Single(fake.SentCommands);
        Assert.Equal(deviceId, sent.DeviceId);

        // Verify the published payload carries the requested duration.
        var json = JsonSerializer.Serialize(sent.Command);
        using var parsed = JsonDocument.Parse(json);
        Assert.Equal("activate_pump", parsed.RootElement.GetProperty("action").GetString());
        Assert.Equal(5, parsed.RootElement.GetProperty("duration_seconds").GetInt32());
    }

    [Fact]
    public async Task TriggerWatering_ClampsDurationAtEightSeconds()
    {
        var (plantId, _) = SeedPlantWithDevice();

        var response = await Client.PostAsJsonAsync(
            $"/api/watering/{plantId}",
            new WateringRequestDto { DurationSeconds = 30 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var fake = Factory.GetMqttFake();
        var sent = Assert.Single(fake.SentCommands);
        var json = JsonSerializer.Serialize(sent.Command);
        using var parsed = JsonDocument.Parse(json);
        Assert.Equal(8, parsed.RootElement.GetProperty("duration_seconds").GetInt32());
    }

    [Fact]
    public async Task TriggerWatering_UsesDefaultDuration_WhenBodyOmitted()
    {
        var (plantId, _) = SeedPlantWithDevice();

        var response = await Client.PostAsJsonAsync(
            $"/api/watering/{plantId}",
            (WateringRequestDto?)null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var fake = Factory.GetMqttFake();
        var sent = Assert.Single(fake.SentCommands);
        var json = JsonSerializer.Serialize(sent.Command);
        using var parsed = JsonDocument.Parse(json);
        // Controller falls back to DEFAULT_DURATION_SECONDS = 5.
        Assert.Equal(5, parsed.RootElement.GetProperty("duration_seconds").GetInt32());
    }

    [Fact]
    public async Task TriggerWatering_Returns404_WhenPlantMissing()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/watering/9999",
            new WateringRequestDto { DurationSeconds = 5 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Empty(Factory.GetMqttFake().SentCommands);
    }

    [Fact]
    public async Task TriggerWatering_Returns400_WhenPlantHasNoDevice()
    {
        int plantId = 0;
        WithDb(db =>
        {
            var plant = new Plant { Name = "Lone Plant" };
            db.Plants.Add(plant);
            db.SaveChanges();
            plantId = plant.Id;
        });

        var response = await Client.PostAsJsonAsync(
            $"/api/watering/{plantId}",
            new WateringRequestDto { DurationSeconds = 5 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(Factory.GetMqttFake().SentCommands);
    }

    [Fact]
    public async Task TriggerWatering_Returns503_WhenMqttPublishFails()
    {
        var (plantId, _) = SeedPlantWithDevice();

        Factory.GetMqttFake().SendCommandShouldSucceed = false;

        var response = await Client.PostAsJsonAsync(
            $"/api/watering/{plantId}",
            new WateringRequestDto { DurationSeconds = 5 });

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    private (int plantId, int deviceId) SeedPlantWithDevice()
    {
        int plantId = 0;
        int deviceId = 0;
        WithDb(db =>
        {
            var plant = new Plant { Name = "Watered Plant" };
            db.Plants.Add(plant);
            db.SaveChanges();
            plantId = plant.Id;

            var device = new Device
            {
                Name = "Test Device",
                CurrentPlantId = plant.Id,
                ApiTokenHash = "hash",
                ApiTokenSalt = "salt"
            };
            db.Devices.Add(device);
            db.SaveChanges();
            deviceId = device.Id;
        });
        return (plantId, deviceId);
    }
}
