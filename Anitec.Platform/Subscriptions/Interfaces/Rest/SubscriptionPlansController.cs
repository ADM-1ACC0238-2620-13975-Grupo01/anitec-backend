using System.Net.Mime;
using Anitec.Platform.Subscriptions.Application.CommandServices;
using Anitec.Platform.Subscriptions.Application.QueryServices;
using Anitec.Platform.Subscriptions.Domain.Model.Commands;
using Anitec.Platform.Subscriptions.Domain.Model.Queries;
using Anitec.Platform.Subscriptions.Interfaces.Rest.Resources;
using Anitec.Platform.Subscriptions.Interfaces.Rest.Transform;
using Anitec.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Anitec.Platform.Subscriptions.Interfaces.Rest;

[Authorize("Rancher", "Veterinarian")]
[ApiController]
[Route("api/v1/subscription-plans")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available SubscriptionPlans endpoints")]
public class SubscriptionPlansController(
    ISubscriptionPlanCommandService commandService,
    ISubscriptionPlanQueryService queryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await queryService.Handle(new GetAllSubscriptionPlansQuery(), cancellationToken);
        return Ok(result.Select(SubscriptionPlanResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await queryService.Handle(new GetSubscriptionPlanByIdQuery(id), cancellationToken);
        if (result is null) return NotFound();
        return Ok(SubscriptionPlanResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSubscriptionPlanResource resource, CancellationToken cancellationToken)
    {
        var command = CreateSubscriptionPlanCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await commandService.Handle(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id },
            SubscriptionPlanResourceFromEntityAssembler.ToResourceFromEntity(result.Value));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateSubscriptionPlanResource resource, CancellationToken cancellationToken)
    {
        var command = UpdateSubscriptionPlanCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await commandService.Handle(command, cancellationToken);
        if (result.IsFailure) return NotFound();
        return Ok(SubscriptionPlanResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new DeleteSubscriptionPlanCommand(id), cancellationToken);
        if (result.IsFailure) return NotFound();
        return NoContent();
    }
}

