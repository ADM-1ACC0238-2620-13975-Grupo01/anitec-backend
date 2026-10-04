using System.Net.Http.Json;
using System.Text.Json;
using Reqnroll;

namespace Anitec.Platform.Tests.Bdd.Steps;

[Binding]
public class HealthRecordSteps(ApiScenarioContext ctx)
{
    [When("I record a {string} health event for the animal {string} with the description {string}")]
    public async Task WhenIRecordAHealthEvent(string type, string tag, string description) =>
        await ctx.SetResponseAsync(await ctx.User!.Http.PostAsJsonAsync("/api/v1/health-events", new
        {
            animalId = ctx.Animals[tag], type, date = "2026-10-01", description, veterinarian = "Dra. Ana Lopez",
            diagnosis = "", treatment = "", prescription = "", followUp = "", nextDueDate = (string?)null
        }));

    [Then("the health event is created")]
    public void ThenTheHealthEventIsCreated() => Assert.Equal(201, (int)ctx.Response!.StatusCode);

    [Then("the health record of the animal {string} includes the description {string}")]
    public async Task ThenTheHealthRecordIncludes(string tag, string description)
    {
        var response = await ctx.User!.Http.GetAsync("/api/v1/health-events");
        response.EnsureSuccessStatusCode();
        var events = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(events.EnumerateArray(), e =>
            e.GetProperty("animalId").GetInt32() == ctx.Animals[tag]
            && e.GetProperty("description").GetString() == description);
    }
}
