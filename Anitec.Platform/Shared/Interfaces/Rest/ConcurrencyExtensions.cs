using Microsoft.AspNetCore.Mvc;

namespace Anitec.Platform.Shared.Interfaces.Rest;

/// <summary>
///     Helpers for the version checks of updates and deletes (US-032, TS-007). A client that edited a record
///     offline sends the version it started from in the <c>If-Match</c> header; if the record has changed
///     since, the API answers 412 with the current copy and changes nothing, so the client can keep both copies
///     and decide which one wins before sending again.
/// </summary>
/// <remarks>
///     <c>If-Match</c> is optional: a request without it behaves as before (the last write wins).
///     The header accepts <c>"3"</c>, <c>W/"3"</c>, a list of them, or <c>*</c> (any version).
/// </remarks>
public static class ConcurrencyExtensions
{
    /// <summary>Writes the <c>ETag</c> header with the version of the record the response is about.</summary>
    public static void SetETag(this ControllerBase controller, int version) =>
        controller.Response.Headers.ETag = $"\"{version}\"";

    /// <summary>
    ///     Returns null when the request may go on, or the 412 response to return when its <c>If-Match</c>
    ///     does not name <paramref name="currentVersion" />. The body has the message, the current version and
    ///     <paramref name="currentResource" />, the server copy of the record.
    /// </summary>
    public static IActionResult? CheckIfMatch(this ControllerBase controller, int currentVersion, object currentResource)
    {
        var header = controller.Request.Headers.IfMatch;
        if (header.Count == 0) return null;

        var current = currentVersion.ToString();
        var matches = header
            .SelectMany(value => (value ?? string.Empty).Split(','))
            .Select(tag => tag.Trim())
            .Any(tag => tag == "*" || StripTag(tag) == current);
        if (matches) return null;

        controller.SetETag(currentVersion);
        return new ObjectResult(new
        {
            message = "The record was changed after the version you based your change on.",
            currentVersion,
            current = currentResource
        })
        {
            StatusCode = StatusCodes.Status412PreconditionFailed
        };
    }

    private static string StripTag(string tag)
    {
        if (tag.StartsWith("W/", StringComparison.OrdinalIgnoreCase)) tag = tag[2..];
        return tag.Trim('"');
    }
}
