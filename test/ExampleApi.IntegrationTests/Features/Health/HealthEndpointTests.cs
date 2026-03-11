using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ExampleApi.IntegrationTests.Common;

namespace ExampleApi.IntegrationTests.Features.Health;

/// <summary>
/// Integration tests for Health endpoint (GET /health).
/// </summary>
public class HealthEndpointTests : IntegrationTestBase
{
    [Fact]
    public async Task Health_ReturnsOkWithHealthyStatus()
    {
        // Act
        var response = await Client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<HealthResponse>();
        result.Should().NotBeNull();
        result!.Status.Should().Be("healthy");
    }

    [Fact]
    public async Task Health_CanBeCalledMultipleTimes()
    {
        // Act
        var response1 = await Client.GetAsync("/health");
        var response2 = await Client.GetAsync("/health");
        var response3 = await Client.GetAsync("/health");

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        response3.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private record HealthResponse(string Status);
}
