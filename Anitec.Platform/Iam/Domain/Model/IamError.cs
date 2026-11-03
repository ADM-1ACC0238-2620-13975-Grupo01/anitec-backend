namespace Anitec.Platform.Iam.Domain.Model;

/// <summary>
/// Failure reasons of the IAM bounded context, translated to localized problem details by the API.
/// </summary>
public enum IamError
{
    None,
    /// <summary>No user has the requested id or username.</summary>
    UserNotFound,
    /// <summary>Sign-up used a username that already exists.</summary>
    UsernameAlreadyTaken,
    /// <summary>Sign-in username or password is wrong.</summary>
    InvalidCredentials,
    /// <summary>The role is not Rancher or Veterinarian.</summary>
    InvalidRole,
    OperationCancelled,
    DatabaseError,
    InternalServerError,
    ExternalServiceError,
    InvalidEmail,
    EmailAlreadyTaken
}
