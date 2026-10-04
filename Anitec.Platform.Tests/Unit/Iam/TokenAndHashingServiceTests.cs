using System.Security.Claims;
using System.Text;
using Anitec.Platform.Iam.Domain.Model.Aggregates;
using Anitec.Platform.Iam.Infrastructure.Hashing.BCrypt.Services;
using Anitec.Platform.Iam.Infrastructure.Tokens.Jwt.Configuration;
using Anitec.Platform.Iam.Infrastructure.Tokens.Jwt.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Anitec.Platform.Tests.Unit.Iam;

public class TokenServiceTests
{
    private const string Secret = "unit-test-secret-key-with-at-least-32-characters";

    private static TokenService CreateService(string secret = Secret) =>
        new(Options.Create(new TokenSettings { Secret = secret }));

    private static readonly User Rancher = new("ana", "hash", "Ana", "Rancher");

    [Fact]
    public void GenerateToken_IncludesTheUsernameAndTheRole()
    {
        var token = CreateService().GenerateToken(Rancher);

        var jwt = new JsonWebToken(token);
        Assert.Equal("ana", jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value);
        Assert.Equal("Rancher", jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
    }

    [Fact]
    public void GenerateToken_ExpiresInSevenDays()
    {
        var token = CreateService().GenerateToken(Rancher);

        var remaining = new JsonWebToken(token).ValidTo - DateTime.UtcNow;
        Assert.InRange(remaining.TotalDays, 6.99, 7.01);
    }

    [Fact]
    public async Task ValidateToken_WithATokenFromTheSameSecret_ReturnsTheUserId()
    {
        var service = CreateService();

        var userId = await service.ValidateToken(service.GenerateToken(Rancher));

        Assert.Equal(Rancher.Id, userId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-jwt")]
    public async Task ValidateToken_WithAMissingOrMalformedToken_ReturnsNull(string? token)
    {
        Assert.Null(await CreateService().ValidateToken(token!));
    }

    [Fact]
    public async Task ValidateToken_WithATokenSignedWithAnotherSecret_ReturnsNull()
    {
        var foreign = CreateService("another-secret-key-with-at-least-32-chars!!").GenerateToken(Rancher);

        Assert.Null(await CreateService().ValidateToken(foreign));
    }

    [Fact]
    public async Task ValidateToken_WithATamperedToken_ReturnsNull()
    {
        var service = CreateService();
        var parts = service.GenerateToken(Rancher).Split('.');
        var forged = $"{parts[0]}.{parts[1]}.{new string('A', parts[2].Length)}";

        Assert.Null(await service.ValidateToken(forged));
    }

    [Fact]
    public async Task ValidateToken_WithAnExpiredToken_ReturnsNull()
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim(ClaimTypes.Sid, "1")]),
            NotBefore = DateTime.UtcNow.AddHours(-2),
            Expires = DateTime.UtcNow.AddHours(-1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Secret)),
                SecurityAlgorithms.HmacSha256Signature)
        };
        var expired = new JsonWebTokenHandler().CreateToken(descriptor);

        Assert.Null(await CreateService().ValidateToken(expired));
    }
}

public class HashingServiceTests
{
    private readonly HashingService _service = new();

    [Fact]
    public void HashPassword_DoesNotStoreThePlainPassword()
    {
        var hash = _service.HashPassword("secret123");

        Assert.NotEqual("secret123", hash);
        Assert.DoesNotContain("secret123", hash);
    }

    [Fact]
    public void HashPassword_ProducesADifferentHashEachTime()
    {
        Assert.NotEqual(_service.HashPassword("secret123"), _service.HashPassword("secret123"));
    }

    [Fact]
    public void VerifyPassword_AcceptsTheOriginalPasswordAndRejectsAnotherOne()
    {
        var hash = _service.HashPassword("secret123");

        Assert.True(_service.VerifyPassword("secret123", hash));
        Assert.False(_service.VerifyPassword("Secret123", hash));
    }
}
