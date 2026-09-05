using Anitec.Platform.Iam.Domain.Model.Commands;
using Anitec.Platform.Iam.Interfaces.Rest.Resources;

// Added for ArgumentNullException

namespace Anitec.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>
///     Assembler responsible for transforming a <see cref="SignUpResource" /> into a <see cref="SignUpCommand" />.
/// </summary>
public static class SignUpCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts a <see cref="SignUpResource" /> to a <see cref="SignUpCommand" />.
    /// </summary>
    /// <param name="resource">
    ///     The <see cref="SignUpResource" /> containing the sign-up data. Must not be null.
    /// </param>
    /// <returns>
    ///     A new <see cref="SignUpCommand" /> instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if the input <paramref name="resource" /> is null.</exception>
    public static SignUpCommand ToCommandFromResource(SignUpResource resource)
    {
        if (resource == null)
            throw new ArgumentNullException(nameof(resource),
                "SignUpResource cannot be null when converting to command.");
        var fullName = string.IsNullOrWhiteSpace(resource.FullName) ? resource.Username : resource.FullName;
        var role = string.IsNullOrWhiteSpace(resource.Role) ? "Rancher" : resource.Role;
        return new SignUpCommand(resource.Username, resource.Password, fullName, role);
    }
}
