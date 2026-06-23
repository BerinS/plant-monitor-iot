using System.Net;
using System.Net.Http.Json;
using PlantMonitoringAPI.DTOs;
using PlantMonitoringAPI.Models;
using Xunit;

namespace PlantMonitoringAPI.Tests.Controllers;

public class PlantsControllerTests : IntegrationTestBase
{
    public PlantsControllerTests(PlantMonitoringWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetPlants_ReturnsEmptyList_WhenNoneExist()
    {
        var response = await Client.GetAsync("/api/plants");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var plants = await response.Content.ReadFromJsonAsync<List<PlantDto>>();
        Assert.NotNull(plants);
        Assert.Empty(plants);
    }

    [Fact]
    public async Task PostPlant_CreatesPlantAndReturns201()
    {
        var request = new CreatePlantDto
        {
            Name = "Test Basil",
            Description = "Kitchen window",
            MoistureThreshold = 35
        };

        var response = await Client.PostAsJsonAsync("/api/plants", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<PlantDto>();
        Assert.NotNull(dto);
        Assert.Equal("Test Basil", dto!.Name);
        Assert.Equal(35, dto.MoistureThreshold);
        Assert.True(dto.Id > 0);
    }

    [Fact]
    public async Task GetPlantById_ReturnsPlant_WhenItExists()
    {
        int plantId = 0;
        WithDb(db =>
        {
            var plant = new Plant { Name = "Existing Plant", MoistureThreshold = 40 };
            db.Plants.Add(plant);
            db.SaveChanges();
            plantId = plant.Id;
        });

        var response = await Client.GetAsync($"/api/plants/{plantId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<PlantDto>();
        Assert.NotNull(dto);
        Assert.Equal("Existing Plant", dto!.Name);
    }

    [Fact]
    public async Task GetPlantById_Returns404_WhenMissing()
    {
        var response = await Client.GetAsync("/api/plants/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutPlant_UpdatesExistingPlant()
    {
        int plantId = 0;
        WithDb(db =>
        {
            var plant = new Plant { Name = "Old Name", MoistureThreshold = 20 };
            db.Plants.Add(plant);
            db.SaveChanges();
            plantId = plant.Id;
        });

        var update = new UpdatePlantDto
        {
            Id = plantId,
            Name = "New Name",
            MoistureThreshold = 45
        };

        var response = await Client.PutAsJsonAsync($"/api/plants/{plantId}", update);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var follow = await Client.GetAsync($"/api/plants/{plantId}");
        var dto = await follow.Content.ReadFromJsonAsync<PlantDto>();
        Assert.Equal("New Name", dto!.Name);
        Assert.Equal(45, dto.MoistureThreshold);
    }

    [Fact]
    public async Task DeletePlant_RemovesPlant()
    {
        int plantId = 0;
        WithDb(db =>
        {
            var plant = new Plant { Name = "To Delete" };
            db.Plants.Add(plant);
            db.SaveChanges();
            plantId = plant.Id;
        });

        var delete = await Client.DeleteAsync($"/api/plants/{plantId}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var follow = await Client.GetAsync($"/api/plants/{plantId}");
        Assert.Equal(HttpStatusCode.NotFound, follow.StatusCode);
    }
}
