using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Anitec.Platform.Tests.Integration;

/// <summary>
///     Each user only reaches their own data: a rancher what they own, a veterinarian what belongs to
///     their accepted clients. Anything else answers 403, and lists leave it out.
/// </summary>
public class AccessScopeApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private Task<ApiTestClient> Rancher() => ApiTestClient.SignedInAsync(factory.CreateClient());

    private Task<ApiTestClient> Vet() => ApiTestClient.SignedInAsync(factory.CreateClient(), "Veterinarian");

    private static List<int> Ids(JsonElement list) =>
        list.EnumerateArray().Select(item => item.GetProperty("id").GetInt32()).ToList();

    private static object Activity(int? ownerId, int? veterinarianId) => new
    {
        ownerId, veterinarianId, title = "Vaccination day", type = "Vacunacion", date = "2026-11-01",
        priority = "Media", status = "Pendiente"
    };

    private static object Record(int ownerId) => new
    {
        ownerId, type = "Income", category = "Sales", amount = 150.5, date = "2026-10-01", description = "Milk"
    };

    private static object Device(int? herdId, int? animalId) => new
    {
        name = "Collar", type = "Collar", serialNumber = $"SN-{Guid.NewGuid():N}", status = "Active", herdId, animalId
    };

    private static async Task<int> CreatedIdAsync(ApiTestClient client, HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await client.ReadAsync(response)).GetProperty("id").GetInt32();
    }

    // ---------- Herds ----------

    [Fact]
    public async Task AHerd_IsHiddenFromOtherRanchers()
    {
        var owner = await Rancher();
        var herdId = await owner.CreateHerdAsync();
        var other = await Rancher();

        var list = await other.ReadAsync(await other.Http.GetAsync("/api/v1/herds"));

        Assert.DoesNotContain(herdId, Ids(list));
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Http.GetAsync($"/api/v1/herds/{herdId}")).StatusCode);
    }

    [Fact]
    public async Task AnotherRancher_CannotChangeOrDeleteAHerd()
    {
        var owner = await Rancher();
        var herdId = await owner.CreateHerdAsync();
        var other = await Rancher();

        var update = await other.Http.PutAsJsonAsync($"/api/v1/herds/{herdId}",
            new { name = "Stolen", location = "Lima", owner = other.Username, ownerId = other.UserId, mainType = "Cattle" });
        var delete = await other.Http.DeleteAsync($"/api/v1/herds/{herdId}");

        Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await owner.Http.GetAsync($"/api/v1/herds/{herdId}")).StatusCode);
    }

    [Fact]
    public async Task ARancher_CannotCreateAHerdForSomeoneElse()
    {
        var rancher = await Rancher();
        var other = await Rancher();

        var response = await rancher.Http.PostAsJsonAsync("/api/v1/herds",
            new { name = "Farm", location = "Cusco", owner = other.Username, ownerId = other.UserId, mainType = "Cattle" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AVeterinarian_SeesTheHerdsOfTheirClientsOnly()
    {
        var client = await Rancher();
        var stranger = await Rancher();
        var clientHerd = await client.CreateHerdAsync();
        var strangerHerd = await stranger.CreateHerdAsync();
        var vet = await Vet();
        await vet.AddClientAsync(client.UserId);

        var ids = Ids(await vet.ReadAsync(await vet.Http.GetAsync("/api/v1/herds")));

        Assert.Contains(clientHerd, ids);
        Assert.DoesNotContain(strangerHerd, ids);
        Assert.Equal(HttpStatusCode.Forbidden, (await vet.Http.GetAsync($"/api/v1/herds/{strangerHerd}")).StatusCode);
    }

    // ---------- Corrals ----------

    [Fact]
    public async Task ACorral_IsHiddenFromOtherRanchers()
    {
        var owner = await Rancher();
        var corralId = await owner.CreateCorralAsync(await owner.CreateHerdAsync());
        var other = await Rancher();

        var list = await other.ReadAsync(await other.Http.GetAsync("/api/v1/corrals"));

        Assert.DoesNotContain(corralId, Ids(list));
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Http.GetAsync($"/api/v1/corrals/{corralId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Http.DeleteAsync($"/api/v1/corrals/{corralId}")).StatusCode);
    }

    [Fact]
    public async Task ARancher_CannotCreateACorralInAnotherRanchersHerd()
    {
        var owner = await Rancher();
        var herdId = await owner.CreateHerdAsync();
        var other = await Rancher();

        var response = await other.Http.PostAsJsonAsync("/api/v1/corrals", new { name = "Intruder", herdId });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ---------- Animals ----------

    [Fact]
    public async Task AnAnimal_IsHiddenFromOtherRanchers()
    {
        var owner = await Rancher();
        var animalId = await owner.CreateAnimalAsync(await owner.CreateHerdAsync(), null, "OWN-1");
        var other = await Rancher();

        var list = await other.ReadAsync(await other.Http.GetAsync("/api/v1/animals"));

        Assert.DoesNotContain(animalId, Ids(list));
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Http.GetAsync($"/api/v1/animals/{animalId}")).StatusCode);
    }

    [Fact]
    public async Task AnotherRancher_CannotChangeOrDeleteAnAnimal()
    {
        var owner = await Rancher();
        var herdId = await owner.CreateHerdAsync();
        var corralId = await owner.CreateCorralAsync(herdId);
        var animalId = await owner.CreateAnimalAsync(herdId, corralId, "OWN-2");
        var other = await Rancher();

        var update = await other.Http.PutAsJsonAsync($"/api/v1/animals/{animalId}",
            ApiTestClient.NewAnimal(herdId, corralId, "OWN-2", "Vendido"));
        var delete = await other.Http.DeleteAsync($"/api/v1/animals/{animalId}");

        Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await owner.Http.GetAsync($"/api/v1/animals/{animalId}")).StatusCode);
    }

    [Fact]
    public async Task ARancher_CannotRegisterAnimalsInAnotherRanchersHerd()
    {
        var owner = await Rancher();
        var herdId = await owner.CreateHerdAsync();
        var corralId = await owner.CreateCorralAsync(herdId);
        var other = await Rancher();

        var single = await other.Http.PostAsJsonAsync("/api/v1/animals", ApiTestClient.NewAnimal(herdId, corralId, "X-1"));
        var bulk = await other.Http.PostAsJsonAsync("/api/v1/animals/bulk", new
        {
            species = "Cattle", breed = "Holstein", gender = "Female", weight = 300, status = "Saludable",
            herdId, corralId, quantity = 2
        });

        Assert.Equal(HttpStatusCode.Forbidden, single.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, bulk.StatusCode);
    }

    [Fact]
    public async Task ARancher_CannotUseTheBulkOperationsOnAnotherRanchersAnimals()
    {
        var owner = await Rancher();
        var animalId = await owner.CreateAnimalAsync(await owner.CreateHerdAsync(), null, "OWN-3");
        var other = await Rancher();

        var status = await other.Http.PatchAsJsonAsync("/api/v1/animals/bulk-status",
            new { animalIds = new[] { animalId }, status = "Vendido" });
        var delete = await other.Http.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/v1/animals/bulk")
        {
            Content = new StringContent(JsonSerializer.Serialize(new { animalIds = new[] { animalId } }), Encoding.UTF8,
                "application/json")
        });

        Assert.Equal(HttpStatusCode.Forbidden, status.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await owner.Http.GetAsync($"/api/v1/animals/{animalId}")).StatusCode);
    }

    // ---------- Health events ----------

    [Fact]
    public async Task AHealthEvent_IsHiddenFromOtherRanchers()
    {
        var owner = await Rancher();
        var animalId = await owner.CreateAnimalAsync(await owner.CreateHerdAsync(), null, "OWN-4");
        var eventId = await CreatedIdAsync(owner, await owner.Http.PostAsJsonAsync("/api/v1/health-events", new
        {
            animalId, type = "Vacuna", date = "2026-10-01", description = "Aftosa", veterinarian = "Dra. Ana",
            diagnosis = "", treatment = "", prescription = "", followUp = "", nextDueDate = (string?)null
        }));
        var other = await Rancher();

        var list = await other.ReadAsync(await other.Http.GetAsync("/api/v1/health-events"));

        Assert.DoesNotContain(eventId, Ids(list));
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Http.GetAsync($"/api/v1/health-events/{eventId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Http.DeleteAsync($"/api/v1/health-events/{eventId}")).StatusCode);
    }

    // ---------- Farm activities ----------

    [Fact]
    public async Task AnActivity_IsHiddenFromOtherRanchers()
    {
        var owner = await Rancher();
        var activityId = await CreatedIdAsync(owner,
            await owner.Http.PostAsJsonAsync("/api/v1/farm-events", Activity(owner.UserId, null)));
        var other = await Rancher();

        var list = await other.ReadAsync(await other.Http.GetAsync("/api/v1/farm-events"));

        Assert.DoesNotContain(activityId, Ids(list));
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Http.GetAsync($"/api/v1/farm-events/{activityId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Http.DeleteAsync($"/api/v1/farm-events/{activityId}")).StatusCode);
    }

    [Fact]
    public async Task ARancher_CannotCreateAnActivityForSomeoneElse()
    {
        var rancher = await Rancher();
        var other = await Rancher();

        var response = await rancher.Http.PostAsJsonAsync("/api/v1/farm-events", Activity(other.UserId, null));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AVeterinarian_CanScheduleAnActivityForAClient_AndTheClientSeesIt()
    {
        var client = await Rancher();
        var vet = await Vet();
        await vet.AddClientAsync(client.UserId);

        var activityId = await CreatedIdAsync(vet,
            await vet.Http.PostAsJsonAsync("/api/v1/farm-events", Activity(client.UserId, vet.UserId)));

        var seenByClient = Ids(await client.ReadAsync(await client.Http.GetAsync("/api/v1/farm-events")));
        Assert.Contains(activityId, seenByClient);
    }

    [Fact]
    public async Task AVeterinarian_CannotScheduleAnActivityForAStranger()
    {
        var stranger = await Rancher();
        var vet = await Vet();

        var response = await vet.Http.PostAsJsonAsync("/api/v1/farm-events", Activity(stranger.UserId, vet.UserId));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ---------- Financial records ----------

    [Fact]
    public async Task AFinancialRecord_IsHiddenFromOtherRanchers()
    {
        var owner = await Rancher();
        var recordId = await CreatedIdAsync(owner,
            await owner.Http.PostAsJsonAsync("/api/v1/financial-records", Record(owner.UserId)));
        var other = await Rancher();

        var list = await other.ReadAsync(await other.Http.GetAsync("/api/v1/financial-records"));

        Assert.DoesNotContain(recordId, Ids(list));
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Http.GetAsync($"/api/v1/financial-records/{recordId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Http.DeleteAsync($"/api/v1/financial-records/{recordId}")).StatusCode);
    }

    [Fact]
    public async Task ARancher_CannotCreateAFinancialRecordForSomeoneElse()
    {
        var rancher = await Rancher();
        var other = await Rancher();

        var response = await rancher.Http.PostAsJsonAsync("/api/v1/financial-records", Record(other.UserId));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ---------- Devices and metrics ----------

    [Fact]
    public async Task ADevice_NeedsAnAssignmentTheUserCanSee()
    {
        var owner = await Rancher();
        var herdId = await owner.CreateHerdAsync();
        var other = await Rancher();

        var unassigned = await owner.Http.PostAsJsonAsync("/api/v1/devices", Device(null, null));
        var foreign = await other.Http.PostAsJsonAsync("/api/v1/devices", Device(herdId, null));
        var own = await owner.Http.PostAsJsonAsync("/api/v1/devices", Device(herdId, null));

        Assert.Equal(HttpStatusCode.BadRequest, unassigned.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, foreign.StatusCode);
        Assert.Equal(HttpStatusCode.Created, own.StatusCode);
    }

    [Fact]
    public async Task ADevice_AndItsMetrics_AreHiddenFromOtherRanchers()
    {
        var owner = await Rancher();
        var herdId = await owner.CreateHerdAsync();
        var deviceId = await CreatedIdAsync(owner, await owner.Http.PostAsJsonAsync("/api/v1/devices", Device(herdId, null)));
        var metricId = await CreatedIdAsync(owner, await owner.Http.PostAsJsonAsync("/api/v1/device-metrics",
            new { deviceId, type = "Temperature", value = 38.5, unit = "C", recordedAt = "2026-10-01T10:00:00Z" }));
        var other = await Rancher();

        var devices = Ids(await other.ReadAsync(await other.Http.GetAsync("/api/v1/devices")));
        var metrics = Ids(await other.ReadAsync(await other.Http.GetAsync("/api/v1/device-metrics")));

        Assert.DoesNotContain(deviceId, devices);
        Assert.DoesNotContain(metricId, metrics);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Http.GetAsync($"/api/v1/devices/{deviceId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Http.GetAsync($"/api/v1/devices/{deviceId}/metrics")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Http.GetAsync($"/api/v1/devices/{deviceId}/latest-metric")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Http.GetAsync($"/api/v1/device-metrics/{metricId}")).StatusCode);
        Assert.Contains(metricId, Ids(await owner.ReadAsync(await owner.Http.GetAsync("/api/v1/device-metrics"))));
    }

    // ---------- Veterinarian clients ----------

    [Fact]
    public async Task AVeterinarian_CannotManageAnotherVeterinariansClients()
    {
        var vet = await Vet();
        var otherVet = await Vet();
        var rancher = await Rancher();

        var list = await vet.Http.GetAsync($"/api/v1/veterinarian/{otherVet.UserId}/clients");
        var available = await vet.Http.GetAsync($"/api/v1/veterinarian/{otherVet.UserId}/available-ranchers");
        var add = await vet.Http.PostAsync($"/api/v1/veterinarian/{otherVet.UserId}/clients/{rancher.UserId}", null);
        var remove = await vet.Http.DeleteAsync($"/api/v1/veterinarian/{otherVet.UserId}/clients/{rancher.UserId}");

        Assert.Equal(HttpStatusCode.Forbidden, list.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, available.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, add.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, remove.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await vet.Http.GetAsync($"/api/v1/veterinarian/{vet.UserId}/clients")).StatusCode);
    }

    // ---------- Dashboards ----------

    [Fact]
    public async Task ARancherDashboard_IsOnlyForThatRancher()
    {
        var rancher = await Rancher();
        var other = await Rancher();
        var vet = await Vet();
        await vet.AddClientAsync(rancher.UserId);

        Assert.Equal(HttpStatusCode.OK, (await rancher.Http.GetAsync($"/api/v1/analytics/ranchers/{rancher.UserId}/dashboard")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Http.GetAsync($"/api/v1/analytics/ranchers/{rancher.UserId}/dashboard")).StatusCode);
        // The dashboard shows income and expenses, so not even a client's veterinarian reads it.
        Assert.Equal(HttpStatusCode.Forbidden, (await vet.Http.GetAsync($"/api/v1/analytics/ranchers/{rancher.UserId}/dashboard")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await vet.Http.GetAsync($"/api/v1/analytics/ranchers/{rancher.UserId}/financial-summary")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Http.GetAsync($"/api/v1/analytics/ranchers/{rancher.UserId}/financial-summary")).StatusCode);
    }

    [Fact]
    public async Task AHealthSummary_IsForTheRancherOrTheirVeterinarian()
    {
        var rancher = await Rancher();
        var other = await Rancher();
        var clientVet = await Vet();
        var strangerVet = await Vet();
        await clientVet.AddClientAsync(rancher.UserId);
        var url = $"/api/v1/analytics/ranchers/{rancher.UserId}/health-summary";

        Assert.Equal(HttpStatusCode.OK, (await rancher.Http.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await clientVet.Http.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Http.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await strangerVet.Http.GetAsync(url)).StatusCode);
    }

    [Fact]
    public async Task AVeterinarianDashboard_IsOnlyForThatVeterinarian()
    {
        var vet = await Vet();
        var otherVet = await Vet();

        Assert.Equal(HttpStatusCode.OK, (await vet.Http.GetAsync($"/api/v1/analytics/veterinarians/{vet.UserId}/dashboard")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await otherVet.Http.GetAsync($"/api/v1/analytics/veterinarians/{vet.UserId}/dashboard")).StatusCode);
    }
}
