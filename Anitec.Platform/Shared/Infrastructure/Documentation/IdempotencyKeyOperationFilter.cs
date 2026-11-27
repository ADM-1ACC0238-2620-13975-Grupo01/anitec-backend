using Anitec.Platform.Shared.Infrastructure.Pipeline.Middleware.Components;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Anitec.Platform.Shared.Infrastructure.Documentation;

/// <summary>
///     Documents the optional <c>Idempotency-Key</c> header on every POST operation in Swagger, so a client
///     can see that a request may be repeated safely.
/// </summary>
public class IdempotencyKeyOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!HttpMethods.IsPost(context.ApiDescription.HttpMethod ?? string.Empty)) return;

        operation.Parameters ??= [];
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = IdempotencyMiddleware.KeyHeader,
            In = ParameterLocation.Header,
            Required = false,
            Description = "Unique value (for example a UUID, up to 100 characters) that identifies this operation. "
                          + "Sending the same key and the same request again returns the stored result of the first "
                          + "one (header Idempotent-Replayed: true) instead of repeating the effect. "
                          + "Results are kept for 24 hours.",
            Schema = new OpenApiSchema { Type = JsonSchemaType.String, MaxLength = IdempotencyMiddleware.MaxKeyLength }
        });
    }
}
