using Anitec.Platform.Iam.Application.CommandServices;
using Anitec.Platform.Iam.Application.Internal.OutboundServices;
using Anitec.Platform.Iam.Domain.Model;
using Anitec.Platform.Iam.Domain.Model.Aggregates;
using Anitec.Platform.Iam.Domain.Model.Commands;
using Anitec.Platform.Iam.Domain.Repositories;
using Anitec.Platform.Resources.Errors;
using Anitec.Platform.Shared.Application.Model;
using Anitec.Platform.Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

// For IamError enum

namespace Anitec.Platform.Iam.Application.Internal.CommandServices;

/**
 * <summary>
 *     The user command service
 * </summary>
 * <remarks>
 *     This class is used to handle user commands
 * </remarks>
 */
public class UserCommandService(
    IUserRepository userRepository,
    ITokenService tokenService,
    IHashingService hashingService,
    IUnitOfWork unitOfWork,
    IStringLocalizer<ErrorMessages> localizer) // Inject IStringLocalizer
    : IUserCommandService
{
    private readonly IStringLocalizer<ErrorMessages> _localizer = localizer;

    /**
     * <summary>
     *     Handle sign in command
     * </summary>
     * <param name="command">The sign in command</param>
     * <param name="cancellationToken">The cancellation token</param>
     * <returns>The authenticated user and the JWT token</returns>
     */
    public async Task<Result<(User user, string token)>> Handle(SignInCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.FindByUsernameAsync(command.Username, cancellationToken);

        if (user == null || !hashingService.VerifyPassword(command.Password, user.PasswordHash))
            return Result<(User user, string token)>.Failure(IamError.InvalidCredentials,
                _localizer[nameof(IamError.InvalidCredentials)]);

        var token = tokenService.GenerateToken(user);

        return Result<(User user, string token)>.Success((user, token));
    }

    /**
     * <summary>
     *     Handle sign up command
     * </summary>
     * <param name="command">The sign-up command</param>
     * <param name="cancellationToken">The cancellation token</param>
     * <returns>A confirmation message on successful creation.</returns>
     */
    public async Task<Result> Handle(SignUpCommand command, CancellationToken cancellationToken)
    {
        if (await userRepository.ExistsByUsernameAsync(command.Username, cancellationToken))
            return Result.Failure(IamError.UsernameAlreadyTaken,
                _localizer[nameof(IamError.UsernameAlreadyTaken), command.Username]);

        var role = NormalizeRole(command.Role);

        if (role is null)
            return Result.Failure(IamError.InvalidRole, _localizer[nameof(IamError.InvalidRole)]);

        // The e-mail is optional: blank means "not provided", anything else must be well formed and unused.
        var email = string.IsNullOrWhiteSpace(command.Email) ? null : NormalizeEmail(command.Email);
        if (!string.IsNullOrWhiteSpace(command.Email) && email is null)
            return Result.Failure(IamError.InvalidEmail, _localizer[nameof(IamError.InvalidEmail)]);
        if (email is not null && await userRepository.ExistsByEmailAsync(email, cancellationToken))
            return Result.Failure(IamError.EmailAlreadyTaken, _localizer[nameof(IamError.EmailAlreadyTaken), email]);

        var hashedPassword = hashingService.HashPassword(command.Password);
        var fullName = string.IsNullOrWhiteSpace(command.FullName) ? command.Username : command.FullName;
        var user = new User(command.Username, hashedPassword, fullName, role, email);
        try
        {
            await userRepository.AddAsync(user, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            return Result.Failure(IamError.OperationCancelled, _localizer[nameof(IamError.OperationCancelled)]);
        }
        catch (DbUpdateException)
        {
            // Log the exception details here if an ILogger is injected
            return Result.Failure(IamError.DatabaseError, _localizer[nameof(IamError.DatabaseError)]);
        }
        catch (Exception)
        {
            // Log the exception details here if an ILogger is injected
            return Result.Failure(IamError.InternalServerError, _localizer[nameof(IamError.InternalServerError)]);
        }
    }

    /// <summary>Trims and lower-cases the address; returns null when it is not a plausible e-mail.</summary>
    private static string? NormalizeEmail(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        if (normalized.Length is 0 or > 254) return null;
        return EmailPattern.IsMatch(normalized) ? normalized : null;
    }

    private static readonly System.Text.RegularExpressions.Regex EmailPattern =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", System.Text.RegularExpressions.RegexOptions.Compiled);

    private static string? NormalizeRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role)) return null;
        if (role.Equals("Rancher", StringComparison.OrdinalIgnoreCase)) return "Rancher";
        if (role.Equals("Veterinarian", StringComparison.OrdinalIgnoreCase)) return "Veterinarian";
        return null;
    }
}
