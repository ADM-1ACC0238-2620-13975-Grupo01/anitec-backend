using Anitec.Platform.Iam.Application.Internal.CommandServices;
using Anitec.Platform.Iam.Application.Internal.OutboundServices;
using Anitec.Platform.Iam.Domain.Model;
using Anitec.Platform.Iam.Domain.Model.Aggregates;
using Anitec.Platform.Iam.Domain.Model.Commands;
using Anitec.Platform.Iam.Domain.Repositories;
using Anitec.Platform.Resources.Errors;
using Anitec.Platform.Shared.Domain.Repositories;
using Anitec.Platform.Tests.Unit.Support;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Anitec.Platform.Tests.Unit.Iam;

public class UserCommandServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ITokenService _tokens = Substitute.For<ITokenService>();
    private readonly IHashingService _hashing = Substitute.For<IHashingService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UserCommandService _service;

    public UserCommandServiceTests()
    {
        _hashing.HashPassword(Arg.Any<string>()).Returns(call => "hash:" + call.Arg<string>());
        _service = new UserCommandService(_users, _tokens, _hashing, _unitOfWork, new FakeLocalizer<ErrorMessages>());
    }

    // ---------- Sign up ----------

    [Fact]
    public async Task SignUp_WithValidData_CreatesTheUserWithAHashedPassword()
    {
        User? saved = null;
        await _users.AddAsync(Arg.Do<User>(u => saved = u), Arg.Any<CancellationToken>());

        var result = await _service.Handle(new SignUpCommand("ana", "secret123", "Ana Lopez", "Rancher"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(saved);
        Assert.Equal("ana", saved.Username);
        Assert.Equal("Ana Lopez", saved.FullName);
        Assert.Equal("hash:secret123", saved.PasswordHash);
        await _unitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SignUp_WithATakenUsername_FailsAndDoesNotSave()
    {
        _users.ExistsByUsernameAsync("ana", Arg.Any<CancellationToken>()).Returns(true);

        var result = await _service.Handle(new SignUpCommand("ana", "secret123", "Ana", "Rancher"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(IamError.UsernameAlreadyTaken, result.Error);
        await _users.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("Admin")]
    [InlineData("Farmer")]
    public async Task SignUp_WithAnInvalidRole_Fails(string role)
    {
        var result = await _service.Handle(new SignUpCommand("ana", "secret123", "Ana", role), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(IamError.InvalidRole, result.Error);
    }

    [Theory]
    [InlineData("rancher", "Rancher")]
    [InlineData("VETERINARIAN", "Veterinarian")]
    public async Task SignUp_NormalizesTheRoleToItsCanonicalForm(string input, string expected)
    {
        User? saved = null;
        await _users.AddAsync(Arg.Do<User>(u => saved = u), Arg.Any<CancellationToken>());

        var result = await _service.Handle(new SignUpCommand("ana", "secret123", "Ana", input), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, saved!.Role);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("a@b")]
    [InlineData("a b@c.com")]
    public async Task SignUp_WithAMalformedEmail_Fails(string email)
    {
        var result = await _service.Handle(new SignUpCommand("ana", "secret123", "Ana", "Rancher", email),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(IamError.InvalidEmail, result.Error);
    }

    [Fact]
    public async Task SignUp_WithAnEmailAlreadyInUse_Fails()
    {
        _users.ExistsByEmailAsync("ana@mail.com", Arg.Any<CancellationToken>()).Returns(true);

        var result = await _service.Handle(new SignUpCommand("ana", "secret123", "Ana", "Rancher", "ana@mail.com"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(IamError.EmailAlreadyTaken, result.Error);
    }

    [Fact]
    public async Task SignUp_StoresTheEmailTrimmedAndInLowerCase()
    {
        User? saved = null;
        await _users.AddAsync(Arg.Do<User>(u => saved = u), Arg.Any<CancellationToken>());

        var result = await _service.Handle(
            new SignUpCommand("ana", "secret123", "Ana", "Rancher", "  Ana@Mail.COM "), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("ana@mail.com", saved!.Email);
    }

    [Fact]
    public async Task SignUp_WithABlankEmail_StoresNoEmail()
    {
        User? saved = null;
        await _users.AddAsync(Arg.Do<User>(u => saved = u), Arg.Any<CancellationToken>());

        var result = await _service.Handle(new SignUpCommand("ana", "secret123", "Ana", "Rancher", "   "),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(saved!.Email);
    }

    [Fact]
    public async Task SignUp_WithABlankFullName_UsesTheUsername()
    {
        User? saved = null;
        await _users.AddAsync(Arg.Do<User>(u => saved = u), Arg.Any<CancellationToken>());

        await _service.Handle(new SignUpCommand("ana", "secret123", "", "Rancher"), CancellationToken.None);

        Assert.Equal("ana", saved!.FullName);
    }

    [Fact]
    public async Task SignUp_WhenTheDatabaseFails_ReturnsADatabaseError()
    {
        _unitOfWork.CompleteAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new DbUpdateException("boom"));

        var result = await _service.Handle(new SignUpCommand("ana", "secret123", "Ana", "Rancher"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(IamError.DatabaseError, result.Error);
    }

    // ---------- Sign in ----------

    [Fact]
    public async Task SignIn_WithValidCredentials_ReturnsTheUserAndAToken()
    {
        var user = new User("ana", "hash", "Ana", "Rancher");
        _users.FindByUsernameAsync("ana", Arg.Any<CancellationToken>()).Returns(user);
        _hashing.VerifyPassword("secret123", "hash").Returns(true);
        _tokens.GenerateToken(user).Returns("jwt-token");

        var result = await _service.Handle(new SignInCommand("ana", "secret123"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(user, result.Value.user);
        Assert.Equal("jwt-token", result.Value.token);
    }

    [Fact]
    public async Task SignIn_WithAnUnknownUser_FailsWithInvalidCredentials()
    {
        _users.FindByUsernameAsync("ghost", Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _service.Handle(new SignInCommand("ghost", "secret123"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(IamError.InvalidCredentials, result.Error);
        _tokens.DidNotReceive().GenerateToken(Arg.Any<User>());
    }

    [Fact]
    public async Task SignIn_WithAWrongPassword_FailsWithInvalidCredentials()
    {
        var user = new User("ana", "hash", "Ana", "Rancher");
        _users.FindByUsernameAsync("ana", Arg.Any<CancellationToken>()).Returns(user);
        _hashing.VerifyPassword("wrong", "hash").Returns(false);

        var result = await _service.Handle(new SignInCommand("ana", "wrong"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(IamError.InvalidCredentials, result.Error);
        _tokens.DidNotReceive().GenerateToken(Arg.Any<User>());
    }
}
