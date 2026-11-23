using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Anitec.Platform.Shared.Domain.Model.Entities;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Anitec.Platform.Tests.Integration;

/// <summary>
///     TS-007: a POST sent with an Idempotency-Key produces a single effect however many times it is sent, so the
///     apps can retry an offline change after a lost response without creating it twice.
/// </summary>
public class IdempotencyApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Header = "Idempotency-Key";

    private Task<ApiTestClient> Rancher() => ApiTestClient.SignedInAsync(factory.CreateClient());

    private Task<ApiTestClient> Vet() => ApiTestClient.SignedInAsync(factory.CreateClient(), "Veterinarian");

    private static string NewKey() => Guid.NewGuid().ToString();

    private static object NewEvent(int animalId, string description = "Control") => new
    {
        animalId, type = "Tratamiento", date = "2026-10-01", description, veterinarian = "Dra. Ana Lopez",
        diagnosis = "", treatment = "", prescription = "", followUp = "", nextDueDate = (string?)null
    };

    /// <summary>Sends a POST with the exact JSON text of the body, so the same call produces the same request.</summary>
    private static async Task<HttpResponseMessage> PostAsync(ApiTestClient client, string url, object body, string? key)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };
        if (key is not null) request.Headers.Add(Header, key);
        return await client.Http.SendAsync(request);
    }

    private static bool WasReplayed(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Idempotent-Replayed", out var values) && values.Contains("true");

    private static async Task<List<int>> IdsAsync(ApiTestClient client, string url) =>
        (await client.ReadAsync(await client.Http.GetAsync(url))).EnumerateArray()
        .Select(item => item.GetProperty("id").GetInt32()).ToList();

    private static async Task<int> IdOfAsync(ApiTestClient client, HttpResponseMessage response) =>
        (await client.ReadAsync(response)).GetProperty("id").GetInt32();

    // ---------- One effect, whatever the retries ----------

    [Fact]
    public async Task ARepeatedRequest_WithTheSameKey_CreatesTheRecordOnce_AndReturnsTheSameResult()
    {
        var rancher = await Rancher();
        var animalId = await rancher.CreateAnimalAsync(await rancher.CreateHerdAsync(), null);
        var key = NewKey();
        var body = NewEvent(animalId);

        var first = await PostAsync(rancher, "/api/v1/health-events", body, key);
        var second = await PostAsync(rancher, "/api/v1/health-events", body, key);
        var third = await PostAsync(rancher, "/api/v1/health-events", body, key);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        Assert.Equal(HttpStatusCode.Created, third.StatusCode);
        Assert.False(WasReplayed(first));
        Assert.True(WasReplayed(second));
        Assert.True(WasReplayed(third));
        var id = await IdOfAsync(rancher, first);
        Assert.Equal(id, await IdOfAsync(rancher, second));
        Assert.Equal(id, await IdOfAsync(rancher, third));
        Assert.Equal(first.Headers.Location, second.Headers.Location);
        Assert.Single(await IdsAsync(rancher, "/api/v1/health-events"), existing => existing == id);
        Assert.Single(await IdsAsync(rancher, "/api/v1/health-events"));
    }

    [Fact]
    public async Task WithoutAKey_EveryRequestCreatesItsOwnRecord()
    {
        var rancher = await Rancher();
        var animalId = await rancher.CreateAnimalAsync(await rancher.CreateHerdAsync(), null);
        var body = NewEvent(animalId);

        var first = await PostAsync(rancher, "/api/v1/health-events", body, null);
        var second = await PostAsync(rancher, "/api/v1/health-events", body, null);

        Assert.NotEqual(await IdOfAsync(rancher, first), await IdOfAsync(rancher, second));
        Assert.False(WasReplayed(second));
        Assert.Equal(2, (await IdsAsync(rancher, "/api/v1/health-events")).Count);
    }

    [Fact]
    public async Task DifferentKeys_AreDifferentOperations()
    {
        var rancher = await Rancher();
        var animalId = await rancher.CreateAnimalAsync(await rancher.CreateHerdAsync(), null);
        var body = NewEvent(animalId);

        var first = await PostAsync(rancher, "/api/v1/health-events", body, NewKey());
        var second = await PostAsync(rancher, "/api/v1/health-events", body, NewKey());

        Assert.NotEqual(await IdOfAsync(rancher, first), await IdOfAsync(rancher, second));
        Assert.Equal(2, (await IdsAsync(rancher, "/api/v1/health-events")).Count);
    }

    [Fact]
    public async Task TheKey_IsNotSharedBetweenUsers()
    {
        var first = await Rancher();
        var second = await Rancher();
        var firstAnimal = await first.CreateAnimalAsync(await first.CreateHerdAsync(), null);
        var secondAnimal = await second.CreateAnimalAsync(await second.CreateHerdAsync(), null);
        var key = NewKey();

        var firstResponse = await PostAsync(first, "/api/v1/health-events", NewEvent(firstAnimal), key);
        var secondResponse = await PostAsync(second, "/api/v1/health-events", NewEvent(secondAnimal), key);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
        Assert.False(WasReplayed(secondResponse));
        Assert.NotEqual(await IdOfAsync(first, firstResponse), await IdOfAsync(second, secondResponse));
    }

    // ---------- It works on every record the apps queue ----------

    [Fact]
    public async Task ARepeatedAnimal_IsCreatedOnce()
    {
        var rancher = await Rancher();
        var herdId = await rancher.CreateHerdAsync();
        var corralId = await rancher.CreateCorralAsync(herdId);
        var key = NewKey();
        var body = ApiTestClient.NewAnimal(herdId, corralId, "DUP-1");

        var first = await PostAsync(rancher, "/api/v1/animals", body, key);
        var second = await PostAsync(rancher, "/api/v1/animals", body, key);

        // Without the key the second would be refused as a duplicate tag; with it the first result comes back.
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        Assert.Equal(await IdOfAsync(rancher, first), await IdOfAsync(rancher, second));
        Assert.Single(await IdsAsync(rancher, "/api/v1/animals"));
    }

    [Fact]
    public async Task ARepeatedActivity_IsCreatedOnce()
    {
        var rancher = await Rancher();
        var key = NewKey();
        var body = new
        {
            ownerId = rancher.UserId, veterinarianId = (int?)null, title = "Vaccination day", type = "Vacunacion",
            date = "2026-11-01", priority = "Media", status = "Pendiente"
        };

        var first = await PostAsync(rancher, "/api/v1/farm-events", body, key);
        var second = await PostAsync(rancher, "/api/v1/farm-events", body, key);

        Assert.Equal(await IdOfAsync(rancher, first), await IdOfAsync(rancher, second));
        Assert.Single(await IdsAsync(rancher, "/api/v1/farm-events"));
    }

    [Fact]
    public async Task ARepeatedFinancialRecord_IsCreatedOnce()
    {
        var rancher = await Rancher();
        var key = NewKey();
        var body = new
        {
            ownerId = rancher.UserId, type = "Income", category = "Sales", amount = 150.5, date = "2026-10-01",
            description = "Milk"
        };

        var first = await PostAsync(rancher, "/api/v1/financial-records", body, key);
        var second = await PostAsync(rancher, "/api/v1/financial-records", body, key);

        Assert.Equal(await IdOfAsync(rancher, first), await IdOfAsync(rancher, second));
        Assert.Single(await IdsAsync(rancher, "/api/v1/financial-records"));
    }

    // ---------- Misuse ----------

    [Fact]
    public async Task TheSameKey_WithADifferentRequest_IsRefused()
    {
        var rancher = await Rancher();
        var animalId = await rancher.CreateAnimalAsync(await rancher.CreateHerdAsync(), null);
        var key = NewKey();
        await PostAsync(rancher, "/api/v1/health-events", NewEvent(animalId, "First"), key);

        var other = await PostAsync(rancher, "/api/v1/health-events", NewEvent(animalId, "Another thing"), key);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, other.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace((await rancher.ReadAsync(other)).GetProperty("message").GetString()));
        Assert.Single(await IdsAsync(rancher, "/api/v1/health-events"));
    }

    [Fact]
    public async Task ABlankKey_IsRefused()
    {
        const string key = "   ";
        var rancher = await Rancher();
        var animalId = await rancher.CreateAnimalAsync(await rancher.CreateHerdAsync(), null);

        var response = await PostAsync(rancher, "/api/v1/health-events", NewEvent(animalId), key);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(await IdsAsync(rancher, "/api/v1/health-events"));
    }

    [Fact]
    public async Task AKeyThatIsTooLong_IsRefused()
    {
        var rancher = await Rancher();
        var animalId = await rancher.CreateAnimalAsync(await rancher.CreateHerdAsync(), null);

        var response = await PostAsync(rancher, "/api/v1/health-events", NewEvent(animalId), new string('k', 101));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(await IdsAsync(rancher, "/api/v1/health-events"));
    }

    [Fact]
    public async Task TheKey_IsIgnoredOnRequestsThatAreNotPosts()
    {
        var rancher = await Rancher();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/herds");
        request.Headers.Add(Header, new string('k', 500));

        var response = await rancher.Http.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ---------- A failure does not freeze the key ----------

    [Fact]
    public async Task ARefusedRequest_IsNotStored_SoARetryCanSucceedOnceTheCauseIsFixed()
    {
        var rancher = await Rancher();
        var animalId = await rancher.CreateAnimalAsync(await rancher.CreateHerdAsync(), null);
        var vet = await Vet();
        var key = NewKey();
        var body = NewEvent(animalId);

        // The veterinarian has no access yet, so the first try is refused...
        var refused = await PostAsync(vet, "/api/v1/health-events", body, key);
        Assert.Equal(HttpStatusCode.Forbidden, refused.StatusCode);

        // ...the rancher accepts, and sending the very same operation again now goes through.
        await rancher.AcceptLinkAsync(await vet.RequestClientAsync(rancher.UserId));
        var retry = await PostAsync(vet, "/api/v1/health-events", body, key);

        Assert.Equal(HttpStatusCode.Created, retry.StatusCode);
        Assert.False(WasReplayed(retry));
    }

    // ---------- Stored keys expire, and abandoned ones are freed ----------

    [Fact]
    public async Task AnExpiredKey_RunsTheRequestAgain()
    {
        var rancher = await Rancher();
        var animalId = await rancher.CreateAnimalAsync(await rancher.CreateHerdAsync(), null);
        var key = NewKey();
        var body = NewEvent(animalId);
        var first = await PostAsync(rancher, "/api/v1/health-events", body, key);
        await AgeRecordAsync(rancher.UserId, key, TimeSpan.FromHours(25));

        var again = await PostAsync(rancher, "/api/v1/health-events", body, key);

        Assert.Equal(HttpStatusCode.Created, again.StatusCode);
        Assert.False(WasReplayed(again));
        Assert.NotEqual(await IdOfAsync(rancher, first), await IdOfAsync(rancher, again));
    }

    [Fact]
    public async Task ARequestStillInProgress_AsksTheClientToWait()
    {
        var rancher = await Rancher();
        var animalId = await rancher.CreateAnimalAsync(await rancher.CreateHerdAsync(), null);
        var key = NewKey();
        var body = NewEvent(animalId);
        // Another request took the key a moment ago and has not finished.
        await InsertRecordAsync(rancher.UserId, key, HashOf("POST", "/api/v1/health-events", JsonSerializer.Serialize(body)),
            statusCode: 0, age: TimeSpan.FromSeconds(2));

        var response = await PostAsync(rancher, "/api/v1/health-events", body, key);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.True(response.Headers.Contains("Retry-After"));
        Assert.Empty(await IdsAsync(rancher, "/api/v1/health-events"));
    }

    [Fact]
    public async Task AnAbandonedRequest_FreesItsKey()
    {
        var rancher = await Rancher();
        var animalId = await rancher.CreateAnimalAsync(await rancher.CreateHerdAsync(), null);
        var key = NewKey();
        var body = NewEvent(animalId);
        // A request took the key long ago and never finished (the server stopped, for example).
        await InsertRecordAsync(rancher.UserId, key, HashOf("POST", "/api/v1/health-events", JsonSerializer.Serialize(body)),
            statusCode: 0, age: TimeSpan.FromMinutes(10));

        var response = await PostAsync(rancher, "/api/v1/health-events", body, key);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Single(await IdsAsync(rancher, "/api/v1/health-events"));
    }

    [Fact]
    public async Task AnUnauthenticatedRequest_IsRefusedWhateverTheKey()
    {
        var http = factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/health-events")
        {
            Content = JsonContent.Create(new { animalId = 1 })
        };
        request.Headers.Add(Header, NewKey());

        var response = await http.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---------- Helpers that reach the database ----------

    private static string HashOf(string method, string path, string body)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hash.AppendData(Encoding.UTF8.GetBytes($"{method}\n{path}\n"));
        hash.AppendData(Encoding.UTF8.GetBytes(body));
        return Convert.ToHexString(hash.GetHashAndReset());
    }

    private async Task InsertRecordAsync(int userId, string key, string requestHash, int statusCode, TimeSpan age)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Set<IdempotencyRecord>().Add(new IdempotencyRecord
        {
            UserId = userId, Key = key, RequestHash = requestHash, StatusCode = statusCode,
            CreatedAt = DateTime.UtcNow - age
        });
        await context.SaveChangesAsync();
    }

    private async Task AgeRecordAsync(int userId, string key, TimeSpan age)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var record = await context.Set<IdempotencyRecord>().SingleAsync(item => item.UserId == userId && item.Key == key);
        record.CreatedAt = DateTime.UtcNow - age;
        await context.SaveChangesAsync();
    }
}
