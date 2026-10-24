using System.Net.Mime;
using Anitec.Platform.Financial.Application.CommandServices;
using Anitec.Platform.Financial.Application.QueryServices;
using Anitec.Platform.Financial.Domain.Model.Commands;
using Anitec.Platform.Financial.Domain.Model.Queries;
using Anitec.Platform.Financial.Interfaces.Rest.Resources;
using Anitec.Platform.Financial.Interfaces.Rest.Transform;
using Anitec.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Anitec.Platform.Shared.Interfaces.Rest;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Anitec.Platform.Financial.Interfaces.Rest;

/// <summary>
/// REST API for financial records (income/expense tracking for the ranch).
/// Restricted to Rancher role only; each rancher only reaches their own records and anything else answers 403.
/// </summary>
[Authorize("Rancher")]
[ApiController]
[Route("api/v1/financial-records")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available FinancialRecords endpoints")]
public class FinancialRecordsController(
    IFinancialRecordCommandService commandService,
    IFinancialRecordQueryService queryService) : ControllerBase
{
    /// <summary>Lists the signed-in rancher's financial records.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var userId = this.CurrentUser().Id;
        var result = await queryService.Handle(new GetAllFinancialRecordsQuery(), cancellationToken);
        return Ok(result.Where(record => record.OwnerId == userId)
            .Select(FinancialRecordResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>Gets one financial record by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await queryService.Handle(new GetFinancialRecordByIdQuery(id), cancellationToken);
        if (result is null) return NotFound();
        if (result.OwnerId != this.CurrentUser().Id) return this.Forbidden();
        return Ok(FinancialRecordResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    /// <summary>Creates a new financial record.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateFinancialRecordResource resource, CancellationToken cancellationToken)
    {
        if (resource.OwnerId != this.CurrentUser().Id) return this.Forbidden();

        var command = CreateFinancialRecordCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await commandService.Handle(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id },
            FinancialRecordResourceFromEntityAssembler.ToResourceFromEntity(result.Value));
    }

    /// <summary>Updates an existing financial record by id.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateFinancialRecordResource resource, CancellationToken cancellationToken)
    {
        var existing = await queryService.Handle(new GetFinancialRecordByIdQuery(id), cancellationToken);
        if (existing is null) return NotFound();
        var userId = this.CurrentUser().Id;
        if (existing.OwnerId != userId || resource.OwnerId != userId) return this.Forbidden();

        var command = UpdateFinancialRecordCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await commandService.Handle(command, cancellationToken);
        if (result.IsFailure) return NotFound();
        return Ok(FinancialRecordResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    /// <summary>Deletes a financial record by id.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var existing = await queryService.Handle(new GetFinancialRecordByIdQuery(id), cancellationToken);
        if (existing is null) return NotFound();
        if (existing.OwnerId != this.CurrentUser().Id) return this.Forbidden();

        var result = await commandService.Handle(new DeleteFinancialRecordCommand(id), cancellationToken);
        if (result.IsFailure) return NotFound();
        return NoContent();
    }
}
