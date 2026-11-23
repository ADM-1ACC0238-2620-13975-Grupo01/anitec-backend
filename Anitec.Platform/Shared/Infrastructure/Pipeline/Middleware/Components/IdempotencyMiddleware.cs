using System.Security.Cryptography;
using System.Text;
using Anitec.Platform.Iam.Domain.Model.Aggregates;
using Anitec.Platform.Shared.Domain.Model.Entities;
using Anitec.Platform.Shared.Domain.Repositories;

namespace Anitec.Platform.Shared.Infrastructure.Pipeline.Middleware.Components;

/// <summary>
///     Makes POST requests safe to repeat. When a signed-in user sends a POST with an <c>Idempotency-Key</c>
///     header, the first request runs normally and its successful response is stored; sending the same key and
///     the same request again returns the stored response (marked with <c>Idempotent-Replayed: true</c>)
///     instead of repeating the effect. This is what lets the mobile apps retry an offline change after a lost
///     response without creating it twice.
/// </summary>
/// <remarks>
///     <list type="bullet">
///         <item><description>Without the header the request goes through untouched.</description></item>
///         <item><description>The same key with a different method, path or body answers 422.</description></item>
///         <item><description>The same key while the first request is still running answers 409 with Retry-After.</description></item>
///         <item><description>
///             Only successful (2xx) responses are stored. A refusal or a server failure releases the key, so a retry
///             runs again and can succeed once the cause is fixed (for example after a veterinarian is accepted).
///         </description></item>
///         <item><description>Keys are kept for <see cref="Retention" />; a request stuck for <see cref="StaleAfter" /> is considered abandoned.</description></item>
///     </list>
///     It must run after the request authorization middleware, which identifies the user.
/// </remarks>
public class IdempotencyMiddleware(RequestDelegate next)
{
    /// <summary>Name of the request header that carries the key.</summary>
    public const string KeyHeader = "Idempotency-Key";

    /// <summary>Name of the response header that marks a stored response.</summary>
    public const string ReplayedHeader = "Idempotent-Replayed";

    /// <summary>Longest accepted key, as stored.</summary>
    public const int MaxKeyLength = 100;

    /// <summary>How long a stored result can be replayed.</summary>
    public static readonly TimeSpan Retention = TimeSpan.FromHours(24);

    /// <summary>How long a request may stay "in progress" before its key can be used again.</summary>
    public static readonly TimeSpan StaleAfter = TimeSpan.FromMinutes(5);

    public async Task InvokeAsync(HttpContext context, IIdempotencyRecordRepository repository)
    {
        if (!HttpMethods.IsPost(context.Request.Method)
            || !context.Request.Headers.TryGetValue(KeyHeader, out var values)
            || context.Items["User"] is not User user)
        {
            await next(context);
            return;
        }

        var key = values.Count == 1 ? values[0]?.Trim() : null;
        if (string.IsNullOrEmpty(key) || key.Length > MaxKeyLength)
        {
            await WriteErrorAsync(context, StatusCodes.Status400BadRequest,
                $"The {KeyHeader} header must have one value of 1 to {MaxKeyLength} characters.");
            return;
        }

        var cancellationToken = context.RequestAborted;
        var hash = await HashRequestAsync(context.Request, cancellationToken);

        var existing = await repository.FindAsync(user.Id, key, cancellationToken);
        if (existing is not null && IsExpired(existing))
        {
            await repository.ReleaseAsync(existing.Id, cancellationToken);
            existing = null;
        }

        if (existing is not null)
        {
            await AnswerFromExistingAsync(context, existing, hash);
            return;
        }

        var claim = new IdempotencyRecord { UserId = user.Id, Key = key, RequestHash = hash };
        if (!await repository.TryClaimAsync(claim, cancellationToken))
        {
            // Another request took the key between the lookup and the insert.
            var winner = await repository.FindAsync(user.Id, key, cancellationToken);
            if (winner is null) await WriteInProgressAsync(context);
            else await AnswerFromExistingAsync(context, winner, hash);
            return;
        }

        await RunAndRecordAsync(context, repository, claim, cancellationToken);
    }

    private async Task RunAndRecordAsync(HttpContext context, IIdempotencyRecordRepository repository,
        IdempotencyRecord claim, CancellationToken cancellationToken)
    {
        var original = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await next(context);
        }
        catch
        {
            context.Response.Body = original;
            await repository.ReleaseAsync(claim.Id, CancellationToken.None);
            throw;
        }

        context.Response.Body = original;
        var status = context.Response.StatusCode;
        if (status is >= 200 and < 300)
        {
            await repository.CompleteAsync(
                claim.Id,
                status,
                Encoding.UTF8.GetString(buffer.GetBuffer(), 0, (int)buffer.Length),
                context.Response.ContentType,
                context.Response.Headers.Location.ToString() is { Length: > 0 } location ? location : null,
                CancellationToken.None);
        }
        else
        {
            await repository.ReleaseAsync(claim.Id, CancellationToken.None);
        }

        buffer.Position = 0;
        await buffer.CopyToAsync(original, cancellationToken);
    }

    private static async Task AnswerFromExistingAsync(HttpContext context, IdempotencyRecord existing, string hash)
    {
        if (!string.Equals(existing.RequestHash, hash, StringComparison.Ordinal))
        {
            await WriteErrorAsync(context, StatusCodes.Status422UnprocessableEntity,
                $"The {KeyHeader} was already used with a different request.");
            return;
        }

        if (existing.IsInProgress)
        {
            await WriteInProgressAsync(context);
            return;
        }

        context.Response.StatusCode = existing.StatusCode;
        context.Response.Headers[ReplayedHeader] = "true";
        if (existing.Location is not null) context.Response.Headers.Location = existing.Location;
        if (existing.ContentType is not null) context.Response.ContentType = existing.ContentType;
        if (!string.IsNullOrEmpty(existing.ResponseBody))
            await context.Response.WriteAsync(existing.ResponseBody, context.RequestAborted);
    }

    private static bool IsExpired(IdempotencyRecord record)
    {
        var age = DateTime.UtcNow - record.CreatedAt;
        return age > Retention || record.IsInProgress && age > StaleAfter;
    }

    private static Task WriteInProgressAsync(HttpContext context)
    {
        context.Response.Headers.RetryAfter = "1";
        return WriteErrorAsync(context, StatusCodes.Status409Conflict,
            $"A request with this {KeyHeader} is still being processed.");
    }

    private static Task WriteErrorAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new { message }, context.RequestAborted);
    }

    /// <summary>Hashes the method, the path with its query and the body, leaving the body readable for the controller.</summary>
    private static async Task<string> HashRequestAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hash.AppendData(Encoding.UTF8.GetBytes($"{request.Method}\n{request.Path}{request.QueryString}\n"));

        request.EnableBuffering();
        var chunk = new byte[8192];
        int read;
        while ((read = await request.Body.ReadAsync(chunk, cancellationToken)) > 0)
            hash.AppendData(chunk, 0, read);
        request.Body.Position = 0;

        return Convert.ToHexString(hash.GetHashAndReset());
    }
}
