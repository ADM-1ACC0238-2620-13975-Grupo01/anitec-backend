using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Anitec.Platform.Tests.Integration;
using Reqnroll;

namespace Anitec.Platform.Tests.Bdd.Steps;

[Binding]
public class LivestockSteps(ApiScenarioContext ctx)
{
    [Given("I have a farm called {string}")]
    [Given("a rancher has a farm called {string}")]
    public async Task GivenARancherHasAFarm(string name)
    {
        ctx.Rancher ??= await ApiTestClient.SignedInAsync(SharedApi.Factory.CreateClient());
        ctx.HerdId = await ctx.Rancher.CreateHerdAsync(name);
    }

    [Given("the farm has a corral called {string}")]
    public async Task GivenTheFarmHasACorral(string name) =>
        ctx.Corrals[name] = await ctx.Rancher!.CreateCorralAsync(ctx.HerdId, name);

    [Given("the corral {string} has the animal {string}")]
    public async Task GivenTheCorralHasTheAnimal(string corral, string tag) =>
        ctx.Animals[tag] = await ctx.Rancher!.CreateAnimalAsync(ctx.HerdId, ctx.Corrals[corral], tag);

    [When("I register an animal with the code {string} in {string}")]
    public async Task WhenIRegisterAnAnimal(string tag, string corral) =>
        await ctx.SetResponseAsync(await Actor().PostAsJsonAsync("/api/v1/animals",
            ApiTestClient.NewAnimal(ctx.HerdId, ctx.Corrals[corral], tag)));

    [When("I register an animal without a corral")]
    public async Task WhenIRegisterAnAnimalWithoutACorral() =>
        await ctx.SetResponseAsync(await Actor().PostAsJsonAsync("/api/v1/animals",
            ApiTestClient.NewAnimal(ctx.HerdId, null, "NO-CORRAL")));

    [When("I register {int} animals in bulk in {string}")]
    public async Task WhenIRegisterAnimalsInBulk(int quantity, string corral) =>
        await ctx.SetResponseAsync(await Actor().PostAsJsonAsync("/api/v1/animals/bulk", new
        {
            species = "Cattle", breed = "Holstein", gender = "Female", weight = 300, status = "Saludable",
            herdId = ctx.HerdId, corralId = ctx.Corrals[corral], quantity
        }));

    [When("I mark the animals {string} as {string}")]
    public async Task WhenIMarkTheAnimalsAs(string tags, string status) =>
        await ctx.SetResponseAsync(await Actor().PatchAsJsonAsync("/api/v1/animals/bulk-status",
            new { animalIds = Ids(tags), status }));

    [When("I delete the animal {string}")]
    public async Task WhenIDeleteTheAnimal(string tag) =>
        await ctx.SetResponseAsync(await Actor().DeleteAsync($"/api/v1/animals/{ctx.Animals[tag]}"));

    [When("I delete the animals {string}")]
    public async Task WhenIDeleteTheAnimals(string tags)
    {
        // The bulk delete sends its ids as a JSON body, the way the mobile app calls it.
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/animals/bulk")
        {
            Content = new StringContent(JsonSerializer.Serialize(new { animalIds = Ids(tags) }), Encoding.UTF8,
                "application/json")
        };
        await ctx.SetResponseAsync(await Actor().SendAsync(request));
    }

    [Then("the animal {string} appears in the animal list")]
    public async Task ThenTheAnimalAppears(string tag) => Assert.Contains(tag, await ListTagsAsync());

    [Then("the animal {string} no longer appears in the animal list")]
    public async Task ThenTheAnimalNoLongerAppears(string tag) => Assert.DoesNotContain(tag, await ListTagsAsync());

    [Then("the codes {string} are assigned")]
    public void ThenTheCodesAreAssigned(string codes)
    {
        Assert.Equal(HttpStatusCodeCreated, (int)ctx.Response!.StatusCode);
        var assigned = ctx.Body.EnumerateArray().Select(a => a.GetProperty("tag").GetString());
        Assert.Equal(codes.Split(',', StringSplitOptions.TrimEntries), assigned);
    }

    [Then("the animals {string} have the status {string}")]
    public async Task ThenTheAnimalsHaveTheStatus(string tags, string status)
    {
        foreach (var tag in tags.Split(',', StringSplitOptions.TrimEntries))
        {
            var response = await Actor().GetAsync($"/api/v1/animals/{ctx.Animals[tag]}");
            var animal = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(status, animal.GetProperty("status").GetString());
        }
    }

    private const int HttpStatusCodeCreated = 201;

    private HttpClient Actor() => ctx.User!.Http;

    private List<int> Ids(string tags) =>
        tags.Split(',', StringSplitOptions.TrimEntries).Select(tag => ctx.Animals[tag]).ToList();

    private async Task<List<string?>> ListTagsAsync()
    {
        var response = await Actor().GetAsync("/api/v1/animals");
        response.EnsureSuccessStatusCode();
        var list = await response.Content.ReadFromJsonAsync<JsonElement>();
        return list.EnumerateArray().Select(a => a.GetProperty("tag").GetString()).ToList();
    }
}
