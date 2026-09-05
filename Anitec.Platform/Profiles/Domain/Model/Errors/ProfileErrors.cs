using Anitec.Platform.Shared.Domain.Model;

namespace Anitec.Platform.Profiles.Domain.Model.Errors;

public static class ProfileErrors
{
    public static readonly Error ProfileCreationFailed =
        new("Profiles.ProfileCreationFailed", "An error occurred while creating the profile.");

    public static readonly Error ProfileNotFound =
        new("Profiles.ProfileNotFound", "The specified profile was not found.");
}
