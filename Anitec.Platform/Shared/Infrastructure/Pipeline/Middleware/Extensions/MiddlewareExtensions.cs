using Anitec.Platform.Shared.Infrastructure.Pipeline.Middleware.Components;

namespace Anitec.Platform.Shared.Infrastructure.Pipeline.Middleware.Extensions;

/// <summary>
///     Middleware extensions
/// </summary>
public static class MiddlewareExtensions
{
    /**
     * <summary>
     *     Use the global exception handler middleware
     * </summary>
     * <param name="builder">The application builder</param>
     * <returns>The application builder</returns>
     */
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }

    /// <summary>
    ///     Use the idempotency middleware, which stores and replays the result of POST requests that carry an
    ///     <c>Idempotency-Key</c> header. Register it after the request authorization middleware.
    /// </summary>
    public static IApplicationBuilder UseIdempotency(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<IdempotencyMiddleware>();
    }
}
