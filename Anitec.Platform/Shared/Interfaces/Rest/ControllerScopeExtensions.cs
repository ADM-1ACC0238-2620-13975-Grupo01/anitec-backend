using Anitec.Platform.Iam.Domain.Model.Aggregates;
using Anitec.Platform.Shared.Application.Internal.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anitec.Platform.Shared.Interfaces.Rest;

/// <summary>Helpers that controllers use to restrict a request to what the signed-in user may access.</summary>
public static class ControllerScopeExtensions
{
    /// <summary>
    ///     The user that the authorization middleware stored for this request.
    ///     Only call it from actions protected with the Authorize attribute.
    /// </summary>
    public static User CurrentUser(this ControllerBase controller) =>
        (User)controller.HttpContext.Items["User"]!;

    /// <summary>Resolves the access scope of the signed-in user.</summary>
    public static Task<AccessScope> ScopeAsync(this ControllerBase controller, IAccessScopeService service,
        CancellationToken cancellationToken) =>
        service.GetAsync(controller.CurrentUser(), cancellationToken);

    /// <summary>
    ///     True when the API runs in production. Endpoints that only exist to ease development (a mock
    ///     checkout, raw subscription edits, plan editing) answer 404 there.
    /// </summary>
    public static bool IsProduction(this ControllerBase controller) =>
        controller.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsProduction();

    /// <summary>
    ///     A 403 response. ForbidResult needs an authentication scheme, which this API does not register,
    ///     so a plain status code is used instead.
    /// </summary>
    public static IActionResult Forbidden(this ControllerBase controller) =>
        controller.StatusCode(StatusCodes.Status403Forbidden);
}
