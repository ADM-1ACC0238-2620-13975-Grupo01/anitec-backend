using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Anitec.Platform.Tests.Integration;

public class LivestockApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private Task<ApiTestClient> Rancher() => ApiTestClient.SignedInAsync(factory.CreateClient());

    // ---------- Herds ----------

    [Fact]
    public async Task CreateHerd_ReturnsCreatedAndTheHerdCanBeFetched()
    {
        var rancher = await Rancher();

        var response = await rancher.Http.PostAsJsonAsync("/api/v1/herds",
            new { name = "La Esperanza", location = "Cusco", owner = rancher.Username, ownerId = rancher.UserId, mainType = "Cattle" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var id = (await rancher.ReadAsync(response)).GetProperty("id").GetInt32();
        var fetched = await rancher.Http.GetAsync($"/api/v1/herds/{id}");
        Assert.Equal(HttpStatusCode.OK, fetched.StatusCode);
        Assert.Equal("La Esperanza", (await rancher.ReadAsync(fetched)).GetProperty("name").GetString());
    }

    [Fact]
    public async Task CreateHerd_WithoutAName_ReturnsBadRequest()
    {
        var rancher = await Rancher();

        var response = await rancher.Http.PostAsJsonAsync("/api/v1/herds",
            new { name = "", location = "Cusco", owner = "x", ownerId = rancher.UserId, mainType = "Cattle" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetHerd_ThatDoesNotExist_ReturnsNotFound()
    {
        var rancher = await Rancher();

        var response = await rancher.Http.GetAsync("/api/v1/herds/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- Corrals ----------

    [Fact]
    public async Task CreateCorral_ReturnsCreatedAndTheCorralIsListed()
    {
        var rancher = await Rancher();
        var herdId = await rancher.CreateHerdAsync();

        var corralId = await rancher.CreateCorralAsync(herdId, "Corral Norte");

        var list = await rancher.ReadAsync(await rancher.Http.GetAsync("/api/v1/corrals"));
        Assert.Contains(list.EnumerateArray(), c => c.GetProperty("id").GetInt32() == corralId
                                                    && c.GetProperty("name").GetString() == "Corral Norte");
    }

    [Fact]
    public async Task DeleteCorral_RemovesIt()
    {
        var rancher = await Rancher();
        var corralId = await rancher.CreateCorralAsync(await rancher.CreateHerdAsync());

        var delete = await rancher.Http.DeleteAsync($"/api/v1/corrals/{corralId}");
        var fetched = await rancher.Http.GetAsync($"/api/v1/corrals/{corralId}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, fetched.StatusCode);
    }

    // ---------- Animals ----------

    [Fact]
    public async Task CreateAnimal_ReturnsCreatedWithTheSentData()
    {
        var rancher = await Rancher();
        var herdId = await rancher.CreateHerdAsync();
        var corralId = await rancher.CreateCorralAsync(herdId);

        var response = await rancher.Http.PostAsJsonAsync("/api/v1/animals",
            ApiTestClient.NewAnimal(herdId, corralId, "COW-777"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await rancher.ReadAsync(response);
        Assert.Equal("COW-777", body.GetProperty("tag").GetString());
        Assert.Equal(corralId, body.GetProperty("corralId").GetInt32());
        Assert.Equal(420m, body.GetProperty("weight").GetDecimal());
    }

    [Fact]
    public async Task CreateAnimal_WithoutACorral_ReturnsBadRequest()
    {
        var rancher = await Rancher();
        var herdId = await rancher.CreateHerdAsync();

        var response = await rancher.Http.PostAsJsonAsync("/api/v1/animals", ApiTestClient.NewAnimal(herdId, null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAnimal_AfterCreatingIt_ReturnsOk()
    {
        var rancher = await Rancher();
        var herdId = await rancher.CreateHerdAsync();
        var animalId = await rancher.CreateAnimalAsync(herdId, null, "COW-100");

        var response = await rancher.Http.GetAsync($"/api/v1/animals/{animalId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("COW-100", (await rancher.ReadAsync(response)).GetProperty("tag").GetString());
    }

    [Fact]
    public async Task GetAnimal_ThatDoesNotExist_ReturnsNotFound()
    {
        var rancher = await Rancher();

        Assert.Equal(HttpStatusCode.NotFound, (await rancher.Http.GetAsync("/api/v1/animals/999999")).StatusCode);
    }

    [Fact]
    public async Task UpdateAnimal_ChangesItsStatus()
    {
        var rancher = await Rancher();
        var herdId = await rancher.CreateHerdAsync();
        var corralId = await rancher.CreateCorralAsync(herdId);
        var animalId = await rancher.CreateAnimalAsync(herdId, corralId, "COW-200");

        var update = await rancher.Http.PutAsJsonAsync($"/api/v1/animals/{animalId}",
            ApiTestClient.NewAnimal(herdId, corralId, "COW-200", "En tratamiento"));

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var fetched = await rancher.ReadAsync(await rancher.Http.GetAsync($"/api/v1/animals/{animalId}"));
        Assert.Equal("En tratamiento", fetched.GetProperty("status").GetString());
    }

    [Fact]
    public async Task DeleteAnimal_RemovesIt()
    {
        var rancher = await Rancher();
        var animalId = await rancher.CreateAnimalAsync(await rancher.CreateHerdAsync(), null, "COW-300");

        var delete = await rancher.Http.DeleteAsync($"/api/v1/animals/{animalId}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await rancher.Http.GetAsync($"/api/v1/animals/{animalId}")).StatusCode);
    }

    [Fact]
    public async Task CreateAnimalBatch_CreatesTheRequestedQuantityWithConsecutiveTags()
    {
        var rancher = await Rancher();
        var herdId = await rancher.CreateHerdAsync();
        var corralId = await rancher.CreateCorralAsync(herdId, "Corral B");

        var response = await rancher.Http.PostAsJsonAsync("/api/v1/animals/bulk", new
        {
            species = "Cattle", breed = "Holstein", gender = "Female", weight = 300, status = "Saludable",
            herdId, corralId, quantity = 3
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var tags = (await rancher.ReadAsync(response)).EnumerateArray().Select(a => a.GetProperty("tag").GetString())
            .ToList();
        Assert.Equal(["CorralB-001", "CorralB-002", "CorralB-003"], tags);
    }

    [Fact]
    public async Task UpdateStatusInBulk_ChangesEveryAnimal()
    {
        var rancher = await Rancher();
        var herdId = await rancher.CreateHerdAsync();
        var first = await rancher.CreateAnimalAsync(herdId, null, "BULK-1");
        var second = await rancher.CreateAnimalAsync(herdId, null, "BULK-2");

        var response = await rancher.Http.PatchAsJsonAsync("/api/v1/animals/bulk-status",
            new { animalIds = new[] { first, second }, status = "Vendido" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        foreach (var id in new[] { first, second })
        {
            var animal = await rancher.ReadAsync(await rancher.Http.GetAsync($"/api/v1/animals/{id}"));
            Assert.Equal("Vendido", animal.GetProperty("status").GetString());
        }
    }

    [Fact]
    public async Task DeleteInBulk_RemovesTheSelectedAnimals()
    {
        var rancher = await Rancher();
        var herdId = await rancher.CreateHerdAsync();
        var first = await rancher.CreateAnimalAsync(herdId, null, "DEL-1");
        var second = await rancher.CreateAnimalAsync(herdId, null, "DEL-2");

        // DELETE with a JSON body, the way the mobile app calls it.
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/animals/bulk")
        {
            Content = new StringContent(JsonSerializer.Serialize(new { animalIds = new[] { first, second } }),
                Encoding.UTF8, "application/json")
        };
        var response = await rancher.Http.SendAsync(request);

        Assert.True(response.IsSuccessStatusCode, $"Unexpected status {response.StatusCode}");
        Assert.Equal(HttpStatusCode.NotFound, (await rancher.Http.GetAsync($"/api/v1/animals/{first}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await rancher.Http.GetAsync($"/api/v1/animals/{second}")).StatusCode);
    }

    // ---------- Roles ----------

    [Fact]
    public async Task AVeterinarian_CanReadAnimals()
    {
        var vet = await ApiTestClient.SignedInAsync(factory.CreateClient(), "Veterinarian");

        Assert.Equal(HttpStatusCode.OK, (await vet.Http.GetAsync("/api/v1/animals")).StatusCode);
    }

    [Fact]
    public async Task AVeterinarian_CannotCreateAnimals()
    {
        var rancher = await Rancher();
        var herdId = await rancher.CreateHerdAsync();
        var corralId = await rancher.CreateCorralAsync(herdId);
        var vet = await ApiTestClient.SignedInAsync(factory.CreateClient(), "Veterinarian");

        var response = await vet.Http.PostAsJsonAsync("/api/v1/animals", ApiTestClient.NewAnimal(herdId, corralId));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
