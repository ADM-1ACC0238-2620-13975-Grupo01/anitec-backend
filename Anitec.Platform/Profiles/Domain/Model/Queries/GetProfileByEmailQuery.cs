using Anitec.Platform.Profiles.Domain.Model.ValueObjects;

namespace Anitec.Platform.Profiles.Domain.Model.Queries;

/// <summary>
///     Get Profile by Email Query
/// </summary>
/// <param name="Email">
///     The <see cref="EmailAddress" /> email address of the profile to retrieve
/// </param>
public record GetProfileByEmailQuery(EmailAddress Email);
