using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Anitec.Platform.Tests.Integration;

/// <summary>
///     The consent flow between veterinarians and ranchers (US-020, US-021, US-022): a veterinarian asks, the
///     rancher accepts or rejects, and an accepted veterinarian can be revoked. Only an accepted link gives access.
/// </summary>
public class VeterinarianConsentApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private Task<ApiTestClient> Rancher() => ApiTestClient.SignedInAsync(factory.CreateClient());

    private Task<ApiTestClient> Vet() => ApiTestClient.SignedInAsync(factory.CreateClient(), "Veterinarian");

    private static List<int> Ids(JsonElement list) =>
        list.EnumerateArray().Select(item => item.GetProperty("id").GetInt32()).ToList();

    private static string LinksUrl(ApiTestClient rancher) => $"/api/v1/rancher/{rancher.UserId}/veterinarian-links";

    private static string LinkAction(ApiTestClient rancher, int linkId, string action) =>
        $"{LinksUrl(rancher)}/{linkId}/{action}";

    private static object NewEvent(int animalId) => new
    {
        animalId, type = "Tratamiento", date = "2026-10-01", description = "Control",
        veterinarian = "Dra. Ana Lopez", diagnosis = "", treatment = "", prescription = "", followUp = "",
        nextDueDate = (string?)null
    };

    private static async Task<JsonElement> LinksAsync(ApiTestClient rancher, string? status = null)
    {
        var url = status is null ? LinksUrl(rancher) : $"{LinksUrl(rancher)}?status={status}";
        return await rancher.ReadAsync(await rancher.Http.GetAsync(url));
    }

    // ---------- US-020: receive a request ----------

    [Fact]
    public async Task ARequest_ArrivesAsPendingWithTheIdentityOfTheVeterinarian()
    {
        var rancher = await Rancher();
        var vet = await Vet();

        var linkId = await vet.RequestClientAsync(rancher.UserId);

        var links = await LinksAsync(rancher);
        var link = Assert.Single(links.EnumerateArray());
        Assert.Equal(linkId, link.GetProperty("id").GetInt32());
        Assert.Equal("Pending", link.GetProperty("status").GetString());
        Assert.Equal(vet.UserId, link.GetProperty("veterinarianId").GetInt32());
        Assert.Equal(vet.Username, link.GetProperty("veterinarianUsername").GetString());
        Assert.False(string.IsNullOrWhiteSpace(link.GetProperty("veterinarianName").GetString()));
    }

    [Fact]
    public async Task ARepeatedRequest_DoesNotCreateAnotherLink()
    {
        var rancher = await Rancher();
        var vet = await Vet();
        await vet.RequestClientAsync(rancher.UserId);

        var again = await vet.Http.PostAsync($"/api/v1/veterinarian/{vet.UserId}/clients/{rancher.UserId}", null);

        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
        Assert.Single((await LinksAsync(rancher)).EnumerateArray());
    }

    [Fact]
    public async Task ARequestOnAnActiveLink_IsAlsoRefused()
    {
        var rancher = await Rancher();
        var vet = await Vet();
        await vet.AddClientAsync(rancher);

        var again = await vet.Http.PostAsync($"/api/v1/veterinarian/{vet.UserId}/clients/{rancher.UserId}", null);

        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
        Assert.Single((await LinksAsync(rancher)).EnumerateArray());
    }

    [Fact]
    public async Task ARequest_ToSomeoneWhoIsNotARancher_IsRefused()
    {
        var vet = await Vet();
        var otherVet = await Vet();

        var response = await vet.Http.PostAsync($"/api/v1/veterinarian/{vet.UserId}/clients/{otherVet.UserId}", null);
        var missing = await vet.Http.PostAsync($"/api/v1/veterinarian/{vet.UserId}/clients/999999", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task APendingRequest_GivesNoAccessToTheRanchersData()
    {
        var rancher = await Rancher();
        var herdId = await rancher.CreateHerdAsync();
        var animalId = await rancher.CreateAnimalAsync(herdId, null);
        var vet = await Vet();
        await vet.RequestClientAsync(rancher.UserId);

        Assert.DoesNotContain(herdId, Ids(await vet.ReadAsync(await vet.Http.GetAsync("/api/v1/herds"))));
        Assert.Equal(HttpStatusCode.Forbidden, (await vet.Http.GetAsync($"/api/v1/herds/{herdId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await vet.Http.PostAsJsonAsync("/api/v1/health-events", NewEvent(animalId))).StatusCode);
    }

    [Fact]
    public async Task APendingRequest_DoesNotCountAsAClientInTheDashboard()
    {
        var rancher = await Rancher();
        await rancher.CreateAnimalAsync(await rancher.CreateHerdAsync(), null);
        var vet = await Vet();
        await vet.RequestClientAsync(rancher.UserId);

        var dashboard = await vet.ReadAsync(
            await vet.Http.GetAsync($"/api/v1/analytics/veterinarians/{vet.UserId}/dashboard"));

        Assert.All(dashboard.EnumerateObject(), property => Assert.Equal(0, property.Value.GetInt32()));
    }

    [Fact]
    public async Task TheListOfAvailableRanchers_LeavesOutThoseAlreadyAskedOrAccepted()
    {
        var asked = await Rancher();
        var accepted = await Rancher();
        var free = await Rancher();
        var vet = await Vet();
        await vet.RequestClientAsync(asked.UserId);
        await vet.AddClientAsync(accepted);

        var ids = Ids(await vet.ReadAsync(await vet.Http.GetAsync($"/api/v1/veterinarian/{vet.UserId}/available-ranchers")));

        Assert.Contains(free.UserId, ids);
        Assert.DoesNotContain(asked.UserId, ids);
        Assert.DoesNotContain(accepted.UserId, ids);
    }

    // ---------- US-021: accept or reject ----------

    [Fact]
    public async Task AcceptingARequest_ActivatesTheLinkAndGrantsAccess()
    {
        var rancher = await Rancher();
        var herdId = await rancher.CreateHerdAsync();
        var animalId = await rancher.CreateAnimalAsync(herdId, null);
        var vet = await Vet();
        var linkId = await vet.RequestClientAsync(rancher.UserId);

        var response = await rancher.Http.PostAsync(LinkAction(rancher, linkId, "accept"), null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await rancher.ReadAsync(response);
        Assert.Equal("Accepted", body.GetProperty("status").GetString());
        Assert.NotEqual(JsonValueKind.Null, body.GetProperty("acceptedAt").ValueKind);
        Assert.Contains(herdId, Ids(await vet.ReadAsync(await vet.Http.GetAsync("/api/v1/herds"))));
        Assert.Equal(HttpStatusCode.Created,
            (await vet.Http.PostAsJsonAsync("/api/v1/health-events", NewEvent(animalId))).StatusCode);
    }

    [Fact]
    public async Task RejectingARequest_ClosesItWithoutGrantingAccess()
    {
        var rancher = await Rancher();
        var herdId = await rancher.CreateHerdAsync();
        var vet = await Vet();
        var linkId = await vet.RequestClientAsync(rancher.UserId);

        var response = await rancher.Http.PostAsync(LinkAction(rancher, linkId, "reject"), null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await rancher.ReadAsync(response);
        Assert.Equal("Rejected", body.GetProperty("status").GetString());
        Assert.Empty((await LinksAsync(rancher, "Pending")).EnumerateArray());
        Assert.DoesNotContain(herdId, Ids(await vet.ReadAsync(await vet.Http.GetAsync("/api/v1/herds"))));
        Assert.Equal(HttpStatusCode.Forbidden, (await vet.Http.GetAsync($"/api/v1/herds/{herdId}")).StatusCode);
    }

    [Fact]
    public async Task ARequestAlreadyAnswered_CannotBeAnsweredAgain()
    {
        var rancher = await Rancher();
        var vet = await Vet();
        var linkId = await vet.RequestClientAsync(rancher.UserId);
        await rancher.AcceptLinkAsync(linkId);

        var accept = await rancher.Http.PostAsync(LinkAction(rancher, linkId, "accept"), null);
        var reject = await rancher.Http.PostAsync(LinkAction(rancher, linkId, "reject"), null);

        Assert.Equal(HttpStatusCode.Conflict, accept.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, reject.StatusCode);
    }

    [Fact]
    public async Task ARejectedVeterinarian_CanAskAgain_AndTheLinkIsReused()
    {
        var rancher = await Rancher();
        var vet = await Vet();
        var firstId = await vet.RequestClientAsync(rancher.UserId);
        await rancher.Http.PostAsync(LinkAction(rancher, firstId, "reject"), null);

        var secondId = await vet.RequestClientAsync(rancher.UserId);

        Assert.Equal(firstId, secondId);
        var link = Assert.Single((await LinksAsync(rancher)).EnumerateArray());
        Assert.Equal("Pending", link.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, link.GetProperty("closedAt").ValueKind);
    }

    [Fact]
    public async Task TheListOfLinks_CanBeFilteredByStatus()
    {
        var rancher = await Rancher();
        var acceptedVet = await Vet();
        var pendingVet = await Vet();
        var acceptedLinkId = await acceptedVet.RequestClientAsync(rancher.UserId);
        var pendingLinkId = await pendingVet.RequestClientAsync(rancher.UserId);
        await rancher.AcceptLinkAsync(acceptedLinkId);

        var pending = Ids(await LinksAsync(rancher, "Pending"));
        var accepted = Ids(await LinksAsync(rancher, "accepted"));

        Assert.Equal([pendingLinkId], pending);
        Assert.Equal([acceptedLinkId], accepted);
    }

    // ---------- US-022: revoke ----------

    [Fact]
    public async Task RevokingAccess_StopsTheVeterinarianButKeepsWhatTheyRecorded()
    {
        var rancher = await Rancher();
        var herdId = await rancher.CreateHerdAsync();
        var animalId = await rancher.CreateAnimalAsync(herdId, null);
        var vet = await Vet();
        var linkId = await vet.RequestClientAsync(rancher.UserId);
        await rancher.AcceptLinkAsync(linkId);
        var recorded = await vet.Http.PostAsJsonAsync("/api/v1/health-events", NewEvent(animalId));
        var eventId = (await vet.ReadAsync(recorded)).GetProperty("id").GetInt32();

        var revoke = await rancher.Http.PostAsync(LinkAction(rancher, linkId, "revoke"), null);

        Assert.Equal(HttpStatusCode.OK, revoke.StatusCode);
        Assert.Equal("Revoked", (await rancher.ReadAsync(revoke)).GetProperty("status").GetString());
        // The veterinarian can no longer see or record anything for this rancher...
        Assert.DoesNotContain(herdId, Ids(await vet.ReadAsync(await vet.Http.GetAsync("/api/v1/herds"))));
        Assert.Equal(HttpStatusCode.Forbidden, (await vet.Http.GetAsync($"/api/v1/animals/{animalId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await vet.Http.GetAsync($"/api/v1/health-events/{eventId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await vet.Http.PostAsJsonAsync("/api/v1/health-events", NewEvent(animalId))).StatusCode);
        // ...while the record they wrote is still there for the rancher, under the same author.
        var kept = await rancher.ReadAsync(await rancher.Http.GetAsync($"/api/v1/health-events/{eventId}"));
        Assert.Equal("Dra. Ana Lopez", kept.GetProperty("veterinarian").GetString());
    }

    [Fact]
    public async Task RevokingAccess_AlsoRemovesTheVeterinarianFromTheHerdsThatNameThem()
    {
        var rancher = await Rancher();
        var vet = await Vet();
        var linkId = await vet.RequestClientAsync(rancher.UserId);
        await rancher.AcceptLinkAsync(linkId);
        var herd = await rancher.Http.PostAsJsonAsync("/api/v1/herds",
            new { name = "Herd", location = "Cusco", owner = rancher.Username, ownerId = rancher.UserId, veterinarianId = vet.UserId, mainType = "Cattle" });
        var herdId = (await rancher.ReadAsync(herd)).GetProperty("id").GetInt32();
        Assert.Contains(herdId, Ids(await vet.ReadAsync(await vet.Http.GetAsync("/api/v1/herds"))));

        await rancher.Http.PostAsync(LinkAction(rancher, linkId, "revoke"), null);

        var after = await rancher.ReadAsync(await rancher.Http.GetAsync($"/api/v1/herds/{herdId}"));
        Assert.Equal(JsonValueKind.Null, after.GetProperty("veterinarianId").ValueKind);
        Assert.DoesNotContain(herdId, Ids(await vet.ReadAsync(await vet.Http.GetAsync("/api/v1/herds"))));
    }

    [Fact]
    public async Task RevokingAccess_HidesTheActivitiesAboutThatRancher()
    {
        var rancher = await Rancher();
        var vet = await Vet();
        var linkId = await vet.RequestClientAsync(rancher.UserId);
        await rancher.AcceptLinkAsync(linkId);
        var created = await vet.Http.PostAsJsonAsync("/api/v1/farm-events", new
        {
            ownerId = rancher.UserId, veterinarianId = vet.UserId, title = "Vaccination day", type = "Vacunacion",
            date = "2026-11-01", priority = "Media", status = "Pendiente"
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var activityId = (await vet.ReadAsync(created)).GetProperty("id").GetInt32();
        Assert.Contains(activityId, Ids(await vet.ReadAsync(await vet.Http.GetAsync("/api/v1/farm-events"))));

        await rancher.Http.PostAsync(LinkAction(rancher, linkId, "revoke"), null);

        Assert.DoesNotContain(activityId, Ids(await vet.ReadAsync(await vet.Http.GetAsync("/api/v1/farm-events"))));
        Assert.Contains(activityId, Ids(await rancher.ReadAsync(await rancher.Http.GetAsync("/api/v1/farm-events"))));
    }

    [Fact]
    public async Task RevokingWithoutActiveAccess_ReportsThereIsNone()
    {
        var rancher = await Rancher();
        var vet = await Vet();
        var pendingId = await vet.RequestClientAsync(rancher.UserId);
        var rejectedVet = await Vet();
        var rejectedId = await rejectedVet.RequestClientAsync(rancher.UserId);
        await rancher.Http.PostAsync(LinkAction(rancher, rejectedId, "reject"), null);

        var pending = await rancher.Http.PostAsync(LinkAction(rancher, pendingId, "revoke"), null);
        var rejected = await rancher.Http.PostAsync(LinkAction(rancher, rejectedId, "revoke"), null);
        var unknown = await rancher.Http.PostAsync(LinkAction(rancher, 999999, "revoke"), null);

        Assert.Equal(HttpStatusCode.NotFound, pending.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, rejected.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace((await rancher.ReadAsync(pending)).GetProperty("message").GetString()));
    }

    [Fact]
    public async Task ARevokedVeterinarian_CanAskAgainAndRegainAccessOnlyIfAccepted()
    {
        var rancher = await Rancher();
        var herdId = await rancher.CreateHerdAsync();
        var vet = await Vet();
        var linkId = await vet.RequestClientAsync(rancher.UserId);
        await rancher.AcceptLinkAsync(linkId);
        await rancher.Http.PostAsync(LinkAction(rancher, linkId, "revoke"), null);

        await vet.RequestClientAsync(rancher.UserId);
        Assert.DoesNotContain(herdId, Ids(await vet.ReadAsync(await vet.Http.GetAsync("/api/v1/herds"))));

        await rancher.AcceptLinkAsync(linkId);
        Assert.Contains(herdId, Ids(await vet.ReadAsync(await vet.Http.GetAsync("/api/v1/herds"))));
    }

    // ---------- Who may answer ----------

    [Fact]
    public async Task OnlyTheAddressedRancher_CanAnswerARequest()
    {
        var rancher = await Rancher();
        var other = await Rancher();
        var vet = await Vet();
        var linkId = await vet.RequestClientAsync(rancher.UserId);

        // Another rancher cannot read the list of someone else...
        var list = await other.Http.GetAsync(LinksUrl(rancher));
        // ...nor answer through their own path (the link is not theirs) or through the owner's path.
        var ownPath = await other.Http.PostAsync(LinkAction(other, linkId, "accept"), null);
        var ownersPath = await other.Http.PostAsync(LinkAction(rancher, linkId, "accept"), null);

        Assert.Equal(HttpStatusCode.Forbidden, list.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, ownPath.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, ownersPath.StatusCode);
        Assert.Empty((await LinksAsync(other)).EnumerateArray());
        Assert.Equal("Pending", Assert.Single((await LinksAsync(rancher)).EnumerateArray()).GetProperty("status").GetString());
    }

    [Fact]
    public async Task AVeterinarian_CannotAnswerItsOwnRequest()
    {
        var rancher = await Rancher();
        var vet = await Vet();
        var linkId = await vet.RequestClientAsync(rancher.UserId);

        var accept = await vet.Http.PostAsync($"/api/v1/rancher/{rancher.UserId}/veterinarian-links/{linkId}/accept", null);
        var list = await vet.Http.GetAsync(LinksUrl(rancher));

        Assert.Equal(HttpStatusCode.Forbidden, accept.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, list.StatusCode);
    }

    [Fact]
    public async Task ARancher_CannotAskForAccessAsAVeterinarian()
    {
        var rancher = await Rancher();
        var other = await Rancher();

        var response = await rancher.Http.PostAsync($"/api/v1/veterinarian/{rancher.UserId}/clients/{other.UserId}", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task TheEndpoints_RequireASignedInUser()
    {
        var http = factory.CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await http.GetAsync("/api/v1/rancher/1/veterinarian-links")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await http.PostAsync("/api/v1/rancher/1/veterinarian-links/1/accept", null)).StatusCode);
    }

    [Fact]
    public async Task AVeterinarian_CanWithdrawARequestBeforeItIsAnswered()
    {
        var rancher = await Rancher();
        var vet = await Vet();
        await vet.RequestClientAsync(rancher.UserId);

        var remove = await vet.Http.DeleteAsync($"/api/v1/veterinarian/{vet.UserId}/clients/{rancher.UserId}");

        Assert.Equal(HttpStatusCode.NoContent, remove.StatusCode);
        Assert.Empty((await LinksAsync(rancher)).EnumerateArray());
    }

    [Fact]
    public async Task TheTotalsOfARancher_AreOnlyGivenToTheVeterinarianOnceAccepted()
    {
        var rancher = await Rancher();
        await rancher.CreateAnimalAsync(await rancher.CreateHerdAsync(), null);
        var vet = await Vet();
        var linkId = await vet.RequestClientAsync(rancher.UserId);

        var pending = (await vet.ReadAsync(await vet.Http.GetAsync($"/api/v1/veterinarian/{vet.UserId}/clients"))).EnumerateArray().Single();
        await rancher.AcceptLinkAsync(linkId);
        var accepted = (await vet.ReadAsync(await vet.Http.GetAsync($"/api/v1/veterinarian/{vet.UserId}/clients"))).EnumerateArray().Single();

        Assert.Equal("Pending", pending.GetProperty("status").GetString());
        Assert.Equal(0, pending.GetProperty("herds").GetInt32());
        Assert.Equal(0, pending.GetProperty("animals").GetInt32());
        Assert.Equal(1, accepted.GetProperty("herds").GetInt32());
        Assert.Equal(1, accepted.GetProperty("animals").GetInt32());
    }
}
