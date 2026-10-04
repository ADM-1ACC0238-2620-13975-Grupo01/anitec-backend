using System.Net.Http.Json;
using Anitec.Platform.Tests.Integration;
using Reqnroll;

namespace Anitec.Platform.Tests.Bdd.Steps;

[Binding]
public class CommonSteps(ApiScenarioContext ctx)
{
    [Given("I am a new visitor")]
    public void GivenIAmANewVisitor()
    {
        ctx.Http = SharedApi.Factory.CreateClient();
    }

    [Given("I have an account as a {string}")]
    public async Task GivenIHaveAnAccountAs(string role)
    {
        ctx.Http = SharedApi.Factory.CreateClient();
        await SignUpAsync(role);
    }

    [Given("I am signed in as a {string}")]
    public async Task GivenIAmSignedInAs(string role)
    {
        // The rancher prepared by the Given steps is the same person when the scenario signs in as a rancher.
        if (role == "Rancher" && ctx.Rancher is not null)
        {
            ctx.User = ctx.Rancher;
            return;
        }

        ctx.User = await ApiTestClient.SignedInAsync(SharedApi.Factory.CreateClient(), role);
        if (role == "Rancher") ctx.Rancher = ctx.User;
    }

    [When("I sign up as a {string}")]
    public async Task WhenISignUpAs(string role) => await SignUpAsync(role);

    [When("I sign in with my credentials")]
    public async Task WhenISignInWithMyCredentials() => await SignInAsync(ApiTestClient.Password);

    [When("I sign in with the password {string}")]
    public async Task WhenISignInWithThePassword(string password) => await SignInAsync(password);

    [When("I request the list of animals")]
    public async Task WhenIRequestTheListOfAnimals() => await ctx.SetResponseAsync(await Actor().GetAsync("/api/v1/animals"));

    [When("I request the health records")]
    public async Task WhenIRequestTheHealthRecords() =>
        await ctx.SetResponseAsync(await Actor().GetAsync("/api/v1/health-events"));

    [Then("I receive a session token")]
    public void ThenIReceiveASessionToken()
    {
        Assert.True(ctx.Response!.IsSuccessStatusCode, $"Sign in failed with {ctx.Response.StatusCode}");
        Assert.False(string.IsNullOrWhiteSpace(ctx.Body.GetProperty("token").GetString()));
    }

    [Then("I do not receive a session token")]
    public void ThenIDoNotReceiveASessionToken()
    {
        Assert.False(ctx.Response!.IsSuccessStatusCode);
        Assert.False(ctx.Body.ValueKind == System.Text.Json.JsonValueKind.Object && ctx.Body.TryGetProperty("token", out _));
    }

    [Then("my role is {string}")]
    public void ThenMyRoleIs(string role) => Assert.Equal(role, ctx.Body.GetProperty("role").GetString());

    [Then("the request is rejected with status {int}")]
    public void ThenTheRequestIsRejectedWithStatus(int status) => Assert.Equal(status, (int)ctx.Response!.StatusCode);

    [Then("the request succeeds")]
    public void ThenTheRequestSucceeds() =>
        Assert.True(ctx.Response!.IsSuccessStatusCode, $"Unexpected status {ctx.Response.StatusCode}");

    /// <summary>The client of the signed-in user, or the anonymous visitor.</summary>
    private HttpClient Actor() => ctx.User?.Http ?? ctx.Http;

    private async Task SignUpAsync(string role)
    {
        await ctx.SetResponseAsync(await ctx.Http.PostAsJsonAsync("/api/v1/authentication/sign-up",
            new { username = ctx.Username, password = ApiTestClient.Password, fullName = "Test " + role, role }));
    }

    private async Task SignInAsync(string password)
    {
        await ctx.SetResponseAsync(await ctx.Http.PostAsJsonAsync("/api/v1/authentication/sign-in",
            new { username = ctx.Username, password }));
    }
}
