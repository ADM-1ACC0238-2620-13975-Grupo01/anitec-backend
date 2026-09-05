using Anitec.Platform.Iam.Application.Internal.OutboundServices;
using Anitec.Platform.Iam.Application.QueryServices;
using Anitec.Platform.Iam.Domain.Model.Queries;
using Anitec.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Anitec.Platform.Iam.Infrastructure.Pipeline.Middleware.Components;

/**
 * RequestAuthorizationMiddleware is a custom middleware.
 * This middleware is used to authorize requests.
 * It validates a token is included in the request header and that the token is valid.
 * If the token is valid then it sets the user in HttpContext.Items["User"].
 */
public class RequestAuthorizationMiddleware(RequestDelegate next)
{
    /**
     * InvokeAsync is called by the ASP.NET Core runtime.
     * It is used to authorize requests.
     * It validates a token is included in the request header and that the token is valid.
     * If the token is valid then it sets the user in HttpContext.Items["User"].
     */
    public async Task InvokeAsync(
        HttpContext context,
        IUserQueryService userQueryService,
        ITokenService tokenService)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await next(context);
            return;
        }

        // skip authorization if endpoint is decorated with [AllowAnonymous] attribute
        var endpoint = context.GetEndpoint();
        if (endpoint is null)
        {
            await next(context);
            return;
        }

        var allowAnonymous = endpoint.Metadata
            .Any(m => m.GetType() == typeof(AllowAnonymousAttribute));
        if (allowAnonymous)
        {
            // [AllowAnonymous] attribute is set, so skip authorization
            await next(context);
            return;
        }

        var requiresAuthorization = endpoint.Metadata.Any(m => m.GetType() == typeof(AuthorizeAttribute));
        if (!requiresAuthorization && endpoint.Metadata.GetMetadata<ControllerActionDescriptor>() is { } descriptor)
        {
            requiresAuthorization = descriptor.ControllerTypeInfo.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any()
                                    || descriptor.MethodInfo.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any();
        }

        if (!requiresAuthorization)
        {
            await next(context);
            return;
        }

        // get token from request header
        var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            await WriteUnauthorizedAsync(context);
            return;
        }

        var token = authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorizationHeader["Bearer ".Length..].Trim()
            : authorizationHeader.Trim();

        if (string.IsNullOrWhiteSpace(token))
        {
            await WriteUnauthorizedAsync(context);
            return;
        }

        // validate token
        var userId = await tokenService.ValidateToken(token);

        if (userId == null)
        {
            await WriteUnauthorizedAsync(context);
            return;
        }

        // get user by id
        var getUserByIdQuery = new GetUserByIdQuery(userId.Value);

        // set user in HttpContext.Items["User"]

        var user = await userQueryService.Handle(getUserByIdQuery, context.RequestAborted);
        if (user is null)
        {
            await WriteUnauthorizedAsync(context);
            return;
        }

        context.Items["User"] = user;
        // call next middleware
        await next(context);
    }

    private static async Task WriteUnauthorizedAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"title\":\"Unauthorized\",\"status\":401,\"detail\":\"Missing or invalid token\"}");
    }
}
