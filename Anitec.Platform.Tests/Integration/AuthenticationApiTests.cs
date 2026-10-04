using System.Net;
using System.Net.Http.Json;

namespace Anitec.Platform.Tests.Integration;

public class AuthenticationApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private HttpClient NewClient() => factory.CreateClient();

    private static string NewUsername() => $"user-{Guid.NewGuid():N}"[..20];

    [Fact]
    public async Task SignUp_WithValidData_ReturnsOk()
    {
        var response = await NewClient().PostAsJsonAsync("/api/v1/authentication/sign-up",
            new { username = NewUsername(), password = ApiTestClient.Password, fullName = "Ana Lopez", role = "Rancher" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SignUp_WithARepeatedUsername_ReturnsConflict()
    {
        var http = NewClient();
        var username = NewUsername();
        var body = new { username, password = ApiTestClient.Password, fullName = "Ana", role = "Rancher" };
        await http.PostAsJsonAsync("/api/v1/authentication/sign-up", body);

        var second = await http.PostAsJsonAsync("/api/v1/authentication/sign-up", body);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task SignUp_WithAnInvalidRole_ReturnsBadRequest()
    {
        var response = await NewClient().PostAsJsonAsync("/api/v1/authentication/sign-up",
            new { username = NewUsername(), password = ApiTestClient.Password, fullName = "Ana", role = "Admin" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SignIn_WithValidCredentials_ReturnsTheUserAndAToken()
    {
        var http = NewClient();
        var username = NewUsername();
        await http.PostAsJsonAsync("/api/v1/authentication/sign-up",
            new { username, password = ApiTestClient.Password, fullName = "Ana Lopez", role = "Veterinarian" });

        var response = await http.PostAsJsonAsync("/api/v1/authentication/sign-in",
            new { username, password = ApiTestClient.Password });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        Assert.Equal(username, body.GetProperty("username").GetString());
        Assert.Equal("Veterinarian", body.GetProperty("role").GetString());
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("token").GetString()));
    }

    [Fact]
    public async Task SignIn_WithAWrongPassword_ReturnsBadRequestAndNoToken()
    {
        var http = NewClient();
        var username = NewUsername();
        await http.PostAsJsonAsync("/api/v1/authentication/sign-up",
            new { username, password = ApiTestClient.Password, fullName = "Ana", role = "Rancher" });

        var response = await http.PostAsJsonAsync("/api/v1/authentication/sign-in",
            new { username, password = "wrong-password" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain("token", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("/api/v1/animals")]
    [InlineData("/api/v1/herds")]
    [InlineData("/api/v1/corrals")]
    [InlineData("/api/v1/health-events")]
    public async Task ProtectedEndpoints_WithoutAToken_ReturnUnauthorized(string url)
    {
        var response = await NewClient().GetAsync(url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoints_WithAnInvalidToken_ReturnUnauthorized()
    {
        var http = NewClient();
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "not-a-valid-token");

        var response = await http.GetAsync("/api/v1/animals");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoints_WithAValidToken_ReturnOk()
    {
        var client = await ApiTestClient.SignedInAsync(NewClient());

        var response = await client.Http.GetAsync("/api/v1/animals");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
