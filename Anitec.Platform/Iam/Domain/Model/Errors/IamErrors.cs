using Anitec.Platform.Shared.Domain.Model;

namespace Anitec.Platform.Iam.Domain.Model.Errors;

/// <summary>
/// Predefined IAM errors with a stable code and a default message.
/// </summary>
public static class IamErrors
{
    /// <summary>Returned when sign-in fails.</summary>
    public static readonly Error InvalidCredentials = new("Iam.InvalidCredentials", "Invalid username or password.");

    /// <summary>Returned when sign-up uses an existing username.</summary>
    public static readonly Error UsernameAlreadyTaken =
        new("Iam.UsernameAlreadyTaken", "The specified username is already taken.");

    /// <summary>Returned when the user could not be saved.</summary>
    public static readonly Error UserCreationFailed =
        new("Iam.UserCreationFailed", "An error occurred while creating the user.");
    
}
