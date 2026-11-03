using Anitec.Platform.Iam.Application.CommandServices;
using Anitec.Platform.Iam.Application.QueryServices;
using Anitec.Platform.Iam.Domain.Model.Commands;
using Anitec.Platform.Iam.Domain.Model.Queries;
using Anitec.Platform.Iam.Interfaces.Acl;

namespace Anitec.Platform.Iam.Application.Acl;

/// <summary>
/// Lets other bounded contexts create and look up users without depending on IAM internals.
/// </summary>
public class IamContextFacade(IUserCommandService userCommandService, IUserQueryService userQueryService)
    : IIamContextFacade
{

    /// <summary>
    /// Signs up a rancher, using the username also as full name, and returns the new user id, or 0 when sign-up fails.
    /// </summary>
    public async Task<int> CreateUser(string username, string password, CancellationToken cancellationToken)
    {
        var signUpCommand = new SignUpCommand(username, password, username, "Rancher");
        var signUpResult = await userCommandService.Handle(signUpCommand, cancellationToken);
        if (signUpResult.IsFailure) return 0;
        var getUserByUsernameQuery = new GetUserByUsernameQuery(username);
        var result = await userQueryService.Handle(getUserByUsernameQuery, cancellationToken);
        return result?.Id ?? 0;
    }

    /// <summary>Returns the id of the user with that username, or 0 when it does not exist.</summary>
    public async Task<int> FetchUserIdByUsername(string username, CancellationToken cancellationToken)
    {
        var getUserByUsernameQuery = new GetUserByUsernameQuery(username);
        var result = await userQueryService.Handle(getUserByUsernameQuery, cancellationToken);
        return result?.Id ?? 0;
    }

    /// <summary>Returns the username of the user with that id, or an empty string when it does not exist.</summary>
    public async Task<string> FetchUsernameByUserId(int userId, CancellationToken cancellationToken)
    {
        var getUserByIdQuery = new GetUserByIdQuery(userId);
        var result = await userQueryService.Handle(getUserByIdQuery, cancellationToken);
        return result?.Username ?? string.Empty;
    }
}
