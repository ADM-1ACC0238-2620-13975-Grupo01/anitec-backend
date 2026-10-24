using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Anitec.Platform.Subscriptions.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Anitec.Platform.Tests.Integration;

public class SubscriptionsApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private Task<ApiTestClient> Rancher() => ApiTestClient.SignedInAsync(factory.CreateClient());

    private static object MockCheckout(int userId, int planId) => new { userId, planId, amount = 49, currency = "PEN" };

    /// <summary>Makes sure the plans exist, as they do after the API starts, and returns the id of one of them.</summary>
    private async Task<int> PlanIdAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await AppDbContextSeeder.SeedSubscriptionPlansAsync(context);
        return (await context.Set<SubscriptionPlan>().OrderBy(plan => plan.Id).FirstAsync()).Id;
    }

    private static List<int> Ids(JsonElement list) =>
        list.EnumerateArray().Select(item => item.GetProperty("id").GetInt32()).ToList();

    // ---------- Plans ----------

    [Fact]
    public async Task TheSubscriptionPlans_AreAddedOnlyWhenThereAreNone()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await AppDbContextSeeder.SeedSubscriptionPlansAsync(context);
        await AppDbContextSeeder.SeedSubscriptionPlansAsync(context);

        var plans = await context.Set<SubscriptionPlan>().OrderBy(plan => plan.Price).ToListAsync();
        Assert.Equal(["Ganadero Basico", "Ganadero Pro", "Veterinario Profesional"], plans.Select(plan => plan.Name));
        Assert.Equal([80, 250, 1000], plans.Select(plan => plan.MaxAnimals));
        Assert.All(plans, plan => Assert.True(plan.IsActive));
    }

    // ---------- Ownership ----------

    [Fact]
    public async Task ASubscription_IsHiddenFromOtherUsers()
    {
        var owner = await Rancher();
        var other = await Rancher();
        var checkout = await owner.ReadAsync(
            await owner.Http.PostAsJsonAsync("/api/v1/subscriptions/mock-checkout", MockCheckout(owner.UserId, await PlanIdAsync())));
        var subscriptionId = checkout.GetProperty("subscription").GetProperty("id").GetInt32();

        var list = Ids(await other.ReadAsync(await other.Http.GetAsync("/api/v1/subscriptions")));

        Assert.DoesNotContain(subscriptionId, list);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Http.GetAsync($"/api/v1/subscriptions/{subscriptionId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Http.DeleteAsync($"/api/v1/subscriptions/{subscriptionId}")).StatusCode);
        Assert.Contains(subscriptionId, Ids(await owner.ReadAsync(await owner.Http.GetAsync("/api/v1/subscriptions"))));
    }

    [Fact]
    public async Task TheActiveSubscriptionAndThePayments_AreOnlyForTheirOwner()
    {
        var owner = await Rancher();
        var other = await Rancher();
        Assert.Equal(HttpStatusCode.OK,
            (await owner.Http.PostAsJsonAsync("/api/v1/subscriptions/mock-checkout", MockCheckout(owner.UserId, await PlanIdAsync()))).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await owner.Http.GetAsync($"/api/v1/subscriptions/users/{owner.UserId}/active")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await owner.Http.GetAsync($"/api/v1/subscriptions/users/{owner.UserId}/payments")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Http.GetAsync($"/api/v1/subscriptions/users/{owner.UserId}/active")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Http.GetAsync($"/api/v1/subscriptions/users/{owner.UserId}/payments")).StatusCode);
    }

    [Fact]
    public async Task ARancher_CannotSubscribeSomeoneElse()
    {
        var rancher = await Rancher();
        var other = await Rancher();

        var checkout = await rancher.Http.PostAsJsonAsync("/api/v1/subscriptions/mock-checkout", MockCheckout(other.UserId, await PlanIdAsync()));
        var create = await rancher.Http.PostAsJsonAsync("/api/v1/subscriptions", new
        {
            userId = other.UserId, planId = await PlanIdAsync(), stripeCustomerId = "c", stripeSubscriptionId = "s", status = "Active",
            startedAt = "2026-10-01", endsAt = (string?)null
        });

        Assert.Equal(HttpStatusCode.Forbidden, checkout.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, create.StatusCode);
    }

    // ---------- Return pages for the mobile checkout ----------

    [Fact]
    public async Task TheSuccessPage_OpensTheAppWithTheCheckoutSession()
    {
        var http = factory.CreateClient();

        var response = await http.GetAsync("/api/v1/subscriptions/mobile-return/success?session_id=cs_test_a1B2c3");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("anitec://subscriptions/success?session_id=cs_test_a1B2c3", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task TheSuccessPage_DropsASessionIdWithUnexpectedCharacters()
    {
        var http = factory.CreateClient();

        var response = await http.GetAsync("/api/v1/subscriptions/mobile-return/success?session_id=%22%3E%3Cscript%3E");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("anitec://subscriptions/success\"", html);
        Assert.DoesNotContain("<script>alert", html);
        Assert.DoesNotContain("session_id=", html);
    }

    [Fact]
    public async Task TheCancelPage_OpensTheApp()
    {
        var http = factory.CreateClient();

        var response = await http.GetAsync("/api/v1/subscriptions/mobile-return/cancel");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("anitec://subscriptions/cancel", await response.Content.ReadAsStringAsync());
    }
}
