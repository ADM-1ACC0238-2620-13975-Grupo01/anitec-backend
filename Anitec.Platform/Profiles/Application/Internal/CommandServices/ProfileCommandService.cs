using Anitec.Platform.Profiles.Application.CommandServices;
using Anitec.Platform.Profiles.Domain.Model;
using Anitec.Platform.Profiles.Domain.Model.Aggregates;
using Anitec.Platform.Profiles.Domain.Model.Commands;
using Anitec.Platform.Profiles.Domain.Repositories;
using Anitec.Platform.Resources.Errors;
using Anitec.Platform.Shared.Application.Model;
using Anitec.Platform.Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

// For ProfilesError enum

namespace Anitec.Platform.Profiles.Application.Internal.CommandServices;

/// <summary>
///     Profile command service
/// </summary>
/// <param name="profileRepository">
///     Profile repository
/// </param>
/// <param name="unitOfWork">
///     Unit of work
/// </param>
public class ProfileCommandService(
    IProfileRepository profileRepository,
    IUnitOfWork unitOfWork,
    IStringLocalizer<ErrorMessages> localizer) // Inject IStringLocalizer
    : IProfileCommandService
{
    private readonly IStringLocalizer<ErrorMessages> _localizer = localizer;

    /// <inheritdoc />
    public async Task<Result<Profile>> Handle(CreateProfileCommand command, CancellationToken cancellationToken)
    {
        var profile = new Profile(command);
        try
        {
            await profileRepository.AddAsync(profile, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<Profile>.Success(profile);
        }
        catch (OperationCanceledException)
        {
            return Result<Profile>.Failure(ProfilesError.OperationCancelled,
                _localizer[nameof(ProfilesError.OperationCancelled)]);
        }
        catch (DbUpdateException)
        {
            // Log the exception details here if an ILogger is injected
            return Result<Profile>.Failure(ProfilesError.DatabaseError,
                _localizer[nameof(ProfilesError.DatabaseError)]);
        }
        catch (Exception)
        {
            // Log the exception details here if an ILogger is injected
            return Result<Profile>.Failure(ProfilesError.InternalServerError,
                _localizer[nameof(ProfilesError.InternalServerError)]);
        }
    }
}
