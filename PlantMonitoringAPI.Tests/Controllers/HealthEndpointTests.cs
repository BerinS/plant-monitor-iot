using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace PlantMonitoringAPI.Tests.Controllers;

// The deploy script polls /api/health after every deployment as a liveness check, so a broken
// endpoint would make every deploy fail. This test catches that in CI 
public class HealthEndpointTests : IntegrationTestBase
{
    public HealthEndpointTests(PlantMonitoringWebApplicationFactory factory) : base(factory) { }

    private record HealthResponse(string Status);

    [Fact]
    public async Task Get_ReturnsOk_WithStatusOk()
    {
        var response = await Client.GetAsync("/api/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();

        Assert.NotNull(body);
        Assert.Equal("ok", body!.Status);
    }
}
