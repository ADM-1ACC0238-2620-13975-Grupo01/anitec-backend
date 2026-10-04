using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Anitec.Platform.Tests.Integration;

/// <summary>Small helper that registers users, signs them in and calls the API with their token.</summary>
public class ApiTestClient(HttpClient http)
{
    public const string Password = "Secret#123";

    public HttpClient Http { get; } = http;

    public int UserId { get; private set; }
    public string Username { get; private set; } = string.Empty;

    /// <summary>Registers a new user with a unique name and signs in as that user.</summary>
    public static async Task<ApiTestClient> SignedInAsync(HttpClient http, string role = "Rancher")
    {
        var client = new ApiTestClient(http);
        var username = $"user-{Guid.NewGuid():N}"[..20];

        var signUp = await http.PostAsJsonAsync("/api/v1/authentication/sign-up",
            new { username, password = Password, fullName = "Test " + role, role });
        signUp.EnsureSuccessStatusCode();

        var signIn = await http.PostAsJsonAsync("/api/v1/authentication/sign-in", new { username, password = Password });
        signIn.EnsureSuccessStatusCode();
        var body = await signIn.Content.ReadFromJsonAsync<JsonElement>();

        client.Username = username;
        client.UserId = body.GetProperty("id").GetInt32();
        client.Http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", body.GetProperty("token").GetString());
        return client;
    }

    public async Task<JsonElement> ReadAsync(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();

    /// <summary>Creates a farm owned by this user and returns its id.</summary>
    public async Task<int> CreateHerdAsync(string name = "La Esperanza")
    {
        var response = await Http.PostAsJsonAsync("/api/v1/herds",
            new { name, location = "Cusco", owner = Username, ownerId = UserId, mainType = "Cattle" });
        response.EnsureSuccessStatusCode();
        return (await ReadAsync(response)).GetProperty("id").GetInt32();
    }

    public async Task<int> CreateCorralAsync(int herdId, string name = "Corral A")
    {
        var response = await Http.PostAsJsonAsync("/api/v1/corrals", new { name, herdId });
        response.EnsureSuccessStatusCode();
        return (await ReadAsync(response)).GetProperty("id").GetInt32();
    }

    public async Task<int> CreateAnimalAsync(int herdId, int? corralId, string tag = "COW-001",
        string status = "Saludable")
    {
        // The API requires every animal to belong to a corral.
        corralId ??= await CreateCorralAsync(herdId, "Corral " + tag);
        var response = await Http.PostAsJsonAsync("/api/v1/animals", NewAnimal(herdId, corralId, tag, status));
        response.EnsureSuccessStatusCode();
        return (await ReadAsync(response)).GetProperty("id").GetInt32();
    }

    public static object NewAnimal(int herdId, int? corralId, string tag = "COW-001", string status = "Saludable") =>
        new
        {
            tag, name = "Lola", species = "Cattle", breed = "Holstein", gender = "Female", birthDate = "2024-03-01",
            weight = 420, status, herdId, corralId
        };
}
