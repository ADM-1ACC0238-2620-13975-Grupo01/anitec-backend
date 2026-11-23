using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Anitec.Platform.Tests.Integration;

/// <summary>
///     US-032 and TS-007: every record that the apps edit offline carries a version. An update or delete that
///     names an outdated version in If-Match is refused with 412 and the current copy, and changes nothing, so
///     the client can keep both copies and decide which one wins.
/// </summary>
public class RecordVersioningApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    /// <summary>The four kinds of record, each with what is needed to create one and to build an update of it.</summary>
    public static TheoryData<string> Kinds => new() { "animals", "health-events", "farm-events", "financial-records" };

    private sealed record Setup(string Url, int Id, Func<string, object> UpdateBody, string Field);

    private async Task<ApiTestClient> Rancher() => await ApiTestClient.SignedInAsync(factory.CreateClient());

    private static async Task<Setup> CreateAsync(ApiTestClient rancher, string kind)
    {
        switch (kind)
        {
            case "animals":
            {
                var herdId = await rancher.CreateHerdAsync();
                var corralId = await rancher.CreateCorralAsync(herdId);
                object Body(string name) => new
                {
                    tag = "VER-1", name, species = "Cattle", breed = "Holstein", gender = "Female",
                    birthDate = "2024-03-01", weight = 420, status = "Saludable", herdId, corralId
                };
                var id = await IdOfAsync(rancher, await rancher.Http.PostAsJsonAsync("/api/v1/animals", Body("Original")));
                return new Setup($"/api/v1/animals/{id}", id, Body, "name");
            }
            case "health-events":
            {
                var animalId = await rancher.CreateAnimalAsync(await rancher.CreateHerdAsync(), null);
                object Body(string description) => new
                {
                    animalId, type = "Tratamiento", date = "2026-10-01", description, veterinarian = "Dra. Ana Lopez",
                    diagnosis = "", treatment = "", prescription = "", followUp = "", nextDueDate = (string?)null
                };
                var id = await IdOfAsync(rancher, await rancher.Http.PostAsJsonAsync("/api/v1/health-events", Body("Original")));
                return new Setup($"/api/v1/health-events/{id}", id, Body, "description");
            }
            case "farm-events":
            {
                object Body(string title) => new
                {
                    ownerId = rancher.UserId, veterinarianId = (int?)null, title, type = "Vacunacion",
                    date = "2026-11-01", priority = "Media", status = "Pendiente"
                };
                var id = await IdOfAsync(rancher, await rancher.Http.PostAsJsonAsync("/api/v1/farm-events", Body("Original")));
                return new Setup($"/api/v1/farm-events/{id}", id, Body, "title");
            }
            default:
            {
                object Body(string description) => new
                {
                    ownerId = rancher.UserId, type = "Income", category = "Sales", amount = 150.5, date = "2026-10-01",
                    description
                };
                var id = await IdOfAsync(rancher, await rancher.Http.PostAsJsonAsync("/api/v1/financial-records", Body("Original")));
                return new Setup($"/api/v1/financial-records/{id}", id, Body, "description");
            }
        }
    }

    private static async Task<int> IdOfAsync(ApiTestClient client, HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await client.ReadAsync(response)).GetProperty("id").GetInt32();
    }

    private static Task<HttpResponseMessage> PutAsync(ApiTestClient client, string url, object body, string? ifMatch)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url) { Content = JsonContent.Create(body) };
        if (ifMatch is not null) request.Headers.TryAddWithoutValidation("If-Match", ifMatch);
        return client.Http.SendAsync(request);
    }

    private static Task<HttpResponseMessage> DeleteAsync(ApiTestClient client, string url, string? ifMatch)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        if (ifMatch is not null) request.Headers.TryAddWithoutValidation("If-Match", ifMatch);
        return client.Http.SendAsync(request);
    }

    private static string? ETagOf(HttpResponseMessage response) =>
        response.Headers.TryGetValues("ETag", out var values) ? values.Single() : null;

    private static async Task<JsonElement> GetAsync(ApiTestClient client, string url) =>
        await client.ReadAsync(await client.Http.GetAsync(url));

    // ---------- Versions start at 1 and go up with every change ----------

    [Theory]
    [MemberData(nameof(Kinds))]
    public async Task ANewRecord_StartsAtVersionOne_AndTheApiReturnsItAsETag(string kind)
    {
        var rancher = await Rancher();

        var record = await CreateAsync(rancher, kind);
        var response = await rancher.Http.GetAsync(record.Url);

        Assert.Equal("\"1\"", ETagOf(response));
        Assert.Equal(1, (await rancher.ReadAsync(response)).GetProperty("version").GetInt32());
        var list = await GetAsync(rancher, record.Url[..record.Url.LastIndexOf('/')]);
        Assert.Equal(1, list.EnumerateArray().Single(item => item.GetProperty("id").GetInt32() == record.Id)
            .GetProperty("version").GetInt32());
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public async Task EveryChange_RaisesTheVersionByOne(string kind)
    {
        var rancher = await Rancher();
        var record = await CreateAsync(rancher, kind);

        var first = await PutAsync(rancher, record.Url, record.UpdateBody("Second"), null);
        var second = await PutAsync(rancher, record.Url, record.UpdateBody("Third"), null);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal("\"2\"", ETagOf(first));
        Assert.Equal(2, (await rancher.ReadAsync(first)).GetProperty("version").GetInt32());
        Assert.Equal("\"3\"", ETagOf(second));
        Assert.Equal(3, (await GetAsync(rancher, record.Url)).GetProperty("version").GetInt32());
    }

    // ---------- If-Match ----------

    [Theory]
    [MemberData(nameof(Kinds))]
    public async Task AnUpdate_WithTheCurrentVersion_Succeeds(string kind)
    {
        var rancher = await Rancher();
        var record = await CreateAsync(rancher, kind);

        var response = await PutAsync(rancher, record.Url, record.UpdateBody("Edited"), "\"1\"");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Edited", (await GetAsync(rancher, record.Url)).GetProperty(record.Field).GetString());
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public async Task AnUpdate_WithAnOutdatedVersion_IsRefusedWithTheCurrentCopy_AndChangesNothing(string kind)
    {
        var rancher = await Rancher();
        var record = await CreateAsync(rancher, kind);
        await PutAsync(rancher, record.Url, record.UpdateBody("Changed meanwhile"), "\"1\"");

        var stale = await PutAsync(rancher, record.Url, record.UpdateBody("My offline edit"), "\"1\"");

        Assert.Equal(HttpStatusCode.PreconditionFailed, stale.StatusCode);
        Assert.Equal("\"2\"", ETagOf(stale));
        var body = await rancher.ReadAsync(stale);
        Assert.Equal(2, body.GetProperty("currentVersion").GetInt32());
        // Both references are kept: the server answers with its copy, and the stored record is untouched.
        Assert.Equal("Changed meanwhile", body.GetProperty("current").GetProperty(record.Field).GetString());
        var stored = await GetAsync(rancher, record.Url);
        Assert.Equal("Changed meanwhile", stored.GetProperty(record.Field).GetString());
        Assert.Equal(2, stored.GetProperty("version").GetInt32());
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public async Task AConflict_CanBeResolvedByKeepingMine_SendingTheCurrentVersion(string kind)
    {
        var rancher = await Rancher();
        var record = await CreateAsync(rancher, kind);
        // Another device saved version 2 while this one was offline with version 1.
        await PutAsync(rancher, record.Url, record.UpdateBody("From the other device"), "\"1\"");
        var conflict = await PutAsync(rancher, record.Url, record.UpdateBody("From this device"), "\"1\"");
        Assert.Equal(HttpStatusCode.PreconditionFailed, conflict.StatusCode);

        // "Keep mine": the app resends its copy against the version the server just reported.
        var currentVersion = (await rancher.ReadAsync(conflict)).GetProperty("currentVersion").GetInt32();
        var resolved = await PutAsync(rancher, record.Url, record.UpdateBody("From this device"), $"\"{currentVersion}\"");

        Assert.Equal(HttpStatusCode.OK, resolved.StatusCode);
        var stored = await GetAsync(rancher, record.Url);
        Assert.Equal("From this device", stored.GetProperty(record.Field).GetString());
        Assert.Equal(3, stored.GetProperty("version").GetInt32());
    }

    [Theory]
    [InlineData("*")]
    [InlineData("W/\"1\"")]
    [InlineData("\"7\", \"1\"")]
    public async Task IfMatch_AcceptsAnyVersion_WeakTagsAndLists(string ifMatch)
    {
        var rancher = await Rancher();
        var record = await CreateAsync(rancher, "financial-records");

        var response = await PutAsync(rancher, record.Url, record.UpdateBody("Edited"), ifMatch);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("\"1\"")]
    [InlineData("\"3\"")]
    [InlineData("1")]
    [InlineData("garbage")]
    public async Task IfMatch_NamingAnotherVersion_IsRefused(string ifMatch)
    {
        var rancher = await Rancher();
        var record = await CreateAsync(rancher, "financial-records");
        await PutAsync(rancher, record.Url, record.UpdateBody("Newer"), null); // the record is now at version 2

        var response = await PutAsync(rancher, record.Url, record.UpdateBody("Mine"), ifMatch);

        Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public async Task WithoutIfMatch_TheLastWriteStillWins(string kind)
    {
        var rancher = await Rancher();
        var record = await CreateAsync(rancher, kind);

        await PutAsync(rancher, record.Url, record.UpdateBody("First"), null);
        var second = await PutAsync(rancher, record.Url, record.UpdateBody("Second"), null);

        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal("Second", (await GetAsync(rancher, record.Url)).GetProperty(record.Field).GetString());
    }

    // ---------- Deleting ----------

    [Theory]
    [MemberData(nameof(Kinds))]
    public async Task ADelete_WithAnOutdatedVersion_IsRefused_AndTheRecordStays(string kind)
    {
        var rancher = await Rancher();
        var record = await CreateAsync(rancher, kind);
        await PutAsync(rancher, record.Url, record.UpdateBody("Changed meanwhile"), null);

        var stale = await DeleteAsync(rancher, record.Url, "\"1\"");

        Assert.Equal(HttpStatusCode.PreconditionFailed, stale.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await rancher.Http.GetAsync(record.Url)).StatusCode);

        var current = await DeleteAsync(rancher, record.Url, "\"2\"");
        Assert.Equal(HttpStatusCode.NoContent, current.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await rancher.Http.GetAsync(record.Url)).StatusCode);
    }

    // ---------- Other ways a record changes ----------

    [Fact]
    public async Task AStatusChange_RaisesTheVersion_AndHonoursIfMatch()
    {
        var rancher = await Rancher();
        var record = await CreateAsync(rancher, "animals");

        var stale = await SendPatchAsync(rancher, $"{record.Url}/status", new { status = "Archivado" }, "\"9\"");
        var ok = await SendPatchAsync(rancher, $"{record.Url}/status", new { status = "Archivado" }, "\"1\"");

        Assert.Equal(HttpStatusCode.PreconditionFailed, stale.StatusCode);
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.Equal("\"2\"", ETagOf(ok));
        Assert.Equal(2, (await rancher.ReadAsync(ok)).GetProperty("version").GetInt32());
    }

    [Fact]
    public async Task ARecordOfAnotherUser_StillAnswers403_BeforeAnyVersionIsChecked()
    {
        var owner = await Rancher();
        var record = await CreateAsync(owner, "financial-records");
        var other = await Rancher();

        var response = await PutAsync(other, record.Url, record.UpdateBody("Stolen"), "\"99\"");

        // A mismatched version must not reveal the record to someone who may not see it.
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static Task<HttpResponseMessage> SendPatchAsync(ApiTestClient client, string url, object body, string ifMatch)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, url) { Content = JsonContent.Create(body) };
        request.Headers.TryAddWithoutValidation("If-Match", ifMatch);
        return client.Http.SendAsync(request);
    }
}
