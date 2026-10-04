using System.Text.Json;
using Anitec.Platform.Tests.Integration;
using Reqnroll;

namespace Anitec.Platform.Tests.Bdd.Steps;

/// <summary>One API instance shared by all the scenarios; each scenario uses its own users, so they do not collide.</summary>
public static class SharedApi
{
    private static readonly Lazy<ApiFactory> Instance = new(() => new ApiFactory());

    public static ApiFactory Factory => Instance.Value;

    public static void Shutdown()
    {
        if (Instance.IsValueCreated) Instance.Value.Dispose();
    }
}

[Binding]
public static class ApiHooks
{
    [AfterTestRun]
    public static void AfterTestRun() => SharedApi.Shutdown();
}

/// <summary>State shared by the steps of one scenario. Reqnroll creates it per scenario and injects it.</summary>
public class ApiScenarioContext
{
    /// <summary>The person doing the actions of the scenario (may be anonymous).</summary>
    public HttpClient Http { get; set; } = SharedApi.Factory.CreateClient();

    /// <summary>The signed-in actor, when there is one.</summary>
    public ApiTestClient? User { get; set; }

    /// <summary>The rancher who owns the farm, the corrals and the animals prepared by the Given steps.</summary>
    public ApiTestClient? Rancher { get; set; }

    public string Username { get; set; } = $"user-{Guid.NewGuid():N}"[..20];

    public int HerdId { get; set; }

    public Dictionary<string, int> Corrals { get; } = new();

    public Dictionary<string, int> Animals { get; } = new();

    public HttpResponseMessage? Response { get; set; }

    public JsonElement Body { get; set; }

    public async Task SetResponseAsync(HttpResponseMessage response)
    {
        Response = response;
        var text = await response.Content.ReadAsStringAsync();
        Body = text.TrimStart().StartsWith('{') || text.TrimStart().StartsWith('[')
            ? JsonDocument.Parse(text).RootElement.Clone()
            : default;
    }
}
