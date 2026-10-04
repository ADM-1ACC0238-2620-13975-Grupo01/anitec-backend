using System.Net;
using System.Net.Http.Json;

namespace Anitec.Platform.Tests.Integration;

public class HealthEventsApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static object NewEvent(int animalId, string type = "Incidencia") => new
    {
        animalId, type, date = "2026-10-01", description = "Cojera en la pata trasera",
        veterinarian = "Dra. Ana Lopez", diagnosis = "", treatment = "", prescription = "", followUp = "",
        nextDueDate = (string?)null
    };

    [Fact]
    public async Task CreateHealthEvent_ReturnsCreatedAndTheEventIsListed()
    {
        var rancher = await ApiTestClient.SignedInAsync(factory.CreateClient());
        var animalId = await rancher.CreateAnimalAsync(await rancher.CreateHerdAsync(), null);

        var response = await rancher.Http.PostAsJsonAsync("/api/v1/health-events", NewEvent(animalId));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var id = (await rancher.ReadAsync(response)).GetProperty("id").GetInt32();
        var list = await rancher.ReadAsync(await rancher.Http.GetAsync("/api/v1/health-events"));
        Assert.Contains(list.EnumerateArray(), e => e.GetProperty("id").GetInt32() == id);
    }

    [Fact]
    public async Task GetHealthEvent_ReturnsTheStoredData()
    {
        var rancher = await ApiTestClient.SignedInAsync(factory.CreateClient());
        var animalId = await rancher.CreateAnimalAsync(await rancher.CreateHerdAsync(), null);
        var created = await rancher.ReadAsync(
            await rancher.Http.PostAsJsonAsync("/api/v1/health-events", NewEvent(animalId, "Vacuna")));

        var response = await rancher.Http.GetAsync($"/api/v1/health-events/{created.GetProperty("id").GetInt32()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await rancher.ReadAsync(response);
        Assert.Equal("Vacuna", body.GetProperty("type").GetString());
        Assert.Equal(animalId, body.GetProperty("animalId").GetInt32());
    }

    [Fact]
    public async Task GetHealthEvent_ThatDoesNotExist_ReturnsNotFound()
    {
        var rancher = await ApiTestClient.SignedInAsync(factory.CreateClient());

        Assert.Equal(HttpStatusCode.NotFound, (await rancher.Http.GetAsync("/api/v1/health-events/999999")).StatusCode);
    }

    [Fact]
    public async Task DeleteHealthEvent_RemovesIt()
    {
        var rancher = await ApiTestClient.SignedInAsync(factory.CreateClient());
        var animalId = await rancher.CreateAnimalAsync(await rancher.CreateHerdAsync(), null);
        var id = (await rancher.ReadAsync(await rancher.Http.PostAsJsonAsync("/api/v1/health-events", NewEvent(animalId))))
            .GetProperty("id").GetInt32();

        var delete = await rancher.Http.DeleteAsync($"/api/v1/health-events/{id}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await rancher.Http.GetAsync($"/api/v1/health-events/{id}")).StatusCode);
    }

    [Fact]
    public async Task AVeterinarian_CanRecordAHealthEvent()
    {
        var rancher = await ApiTestClient.SignedInAsync(factory.CreateClient());
        var animalId = await rancher.CreateAnimalAsync(await rancher.CreateHerdAsync(), null);
        var vet = await ApiTestClient.SignedInAsync(factory.CreateClient(), "Veterinarian");

        var response = await vet.Http.PostAsJsonAsync("/api/v1/health-events", NewEvent(animalId, "Tratamiento"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
