using System.Net.Mime;
using Anitec.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Anitec.Platform.Livestock.Application.CommandServices;
using Anitec.Platform.Livestock.Application.QueryServices;
using Anitec.Platform.Livestock.Domain.Model.Commands;
using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Livestock.Domain.Model.Queries;
using Anitec.Platform.Livestock.Interfaces.Rest.Resources;
using Anitec.Platform.Livestock.Interfaces.Rest.Transform;
using Anitec.Platform.Shared.Application.Internal.Authorization;
using Anitec.Platform.Shared.Interfaces.Rest;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Anitec.Platform.Livestock.Interfaces.Rest;

/// <summary>
/// REST API for livestock animals (CRUD, bulk ops, and image upload).
/// Accessible by Rancher and Veterinarian roles; write operations require Rancher.
/// Only the animals of herds the user can see are returned or changed; anything else answers 403.
/// </summary>
[Authorize("Rancher", "Veterinarian")]
[ApiController]
[Route("api/v1/animals")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Animals endpoints")]
public class AnimalsController(
    IAnimalCommandService commandService,
    IAnimalQueryService queryService,
    IWebHostEnvironment webHostEnvironment,
    IAccessScopeService accessScopeService) : ControllerBase
{
    /// <summary>MIME types accepted when uploading an animal image.</summary>
    private static readonly string[] AllowedImageContentTypes = ["image/jpeg", "image/png", "image/webp", "image/gif"];

    /// <summary>
    /// Returns the animals the signed-in user can see. Archived animals are included unless
    /// <paramref name="archived" /> is false; with true only the archived ones are returned.
    /// </summary>
    /// <param name="archived">Optional filter on the archived status.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? archived, CancellationToken cancellationToken)
    {
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        var result = await queryService.Handle(new GetAllAnimalsQuery(), cancellationToken);
        return Ok(result.Where(animal => scope.CanSeeAnimalId(animal.Id))
            .Where(animal => archived is not { } wanted || IsArchived(animal) == wanted)
            .Select(AnimalResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>Returns a single animal by its identifier.</summary>
    /// <param name="id">Animal primary key.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await queryService.Handle(new GetAnimalByIdQuery(id), cancellationToken);
        if (result is null) return NotFound();
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeAnimalId(result.Id)) return this.Forbidden();
        return Ok(AnimalResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    /// <summary>Creates a new animal. Rancher role only.</summary>
    /// <param name="resource">Animal payload from the client.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [Authorize("Rancher")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateAnimalResource resource, CancellationToken cancellationToken)
    {
        var errors = ValidateAnimalResource(resource);
        if (errors.Count > 0) return BadRequest(new { message = "One or more fields are invalid.", errors });
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeHerdId(resource.HerdId)) return this.Forbidden();

        var command = CreateAnimalCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await commandService.Handle(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id },
            AnimalResourceFromEntityAssembler.ToResourceFromEntity(result.Value));
    }

    /// <summary>Updates an existing animal by id. Rancher role only.</summary>
    /// <param name="id">Animal primary key to update.</param>
    /// <param name="resource">Updated animal payload.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [Authorize("Rancher")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateAnimalResource resource, CancellationToken cancellationToken)
    {
        var errors = ValidateAnimalResource(resource);
        if (errors.Count > 0) return BadRequest(new { message = "One or more fields are invalid.", errors });

        var existing = await queryService.Handle(new GetAnimalByIdQuery(id), cancellationToken);
        if (existing is null) return NotFound();
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeAnimalId(existing.Id) || !scope.CanSeeHerdId(resource.HerdId)) return this.Forbidden();

        var command = UpdateAnimalCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await commandService.Handle(command, cancellationToken);
        if (result.IsFailure) return NotFound();
        return Ok(AnimalResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    /// <summary>True when the animal's status is the archived one.</summary>
    private static bool IsArchived(Animal animal) =>
        string.Equals(animal.Status, Animal.ArchivedStatus, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Validates required fields and basic constraints on a create/update animal payload.
    /// Returns a list of human-readable error messages (empty when valid).
    /// </summary>
    private static List<string> ValidateAnimalResource(CreateAnimalResource resource)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(resource.Tag)) errors.Add("Code is required.");
        if (string.IsNullOrWhiteSpace(resource.Name)) errors.Add("Name is required.");
        if (string.IsNullOrWhiteSpace(resource.Species)) errors.Add("Animal type is required.");
        if (string.IsNullOrWhiteSpace(resource.Breed)) errors.Add("Breed is required.");
        if (string.IsNullOrWhiteSpace(resource.Gender)) errors.Add("Gender is required.");
        if (resource.HerdId <= 0) errors.Add("Herd is required.");
        if (resource.CorralId is null or <= 0) errors.Add("Corral is required.");
        if (resource.Weight < 0) errors.Add("Weight cannot be negative.");
        return errors;
    }

    /// <summary>Deletes an animal by id. Rancher role only.</summary>
    /// <param name="id">Animal primary key to delete.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [Authorize("Rancher")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var existing = await queryService.Handle(new GetAnimalByIdQuery(id), cancellationToken);
        if (existing is null) return NotFound();
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeAnimalId(existing.Id)) return this.Forbidden();

        var result = await commandService.Handle(new DeleteAnimalCommand(id), cancellationToken);
        if (result.IsFailure) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Creates multiple animals in one request (same species/breed/gender/herd/corral).
    /// Quantity must be between 1 and 500. Rancher role only.
    /// </summary>
    [Authorize("Rancher")]
    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBatch(CreateAnimalBatchResource resource, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(resource.Species)) errors.Add("Animal type is required.");
        if (string.IsNullOrWhiteSpace(resource.Breed)) errors.Add("Breed is required.");
        if (string.IsNullOrWhiteSpace(resource.Gender)) errors.Add("Gender is required.");
        if (resource.HerdId <= 0) errors.Add("Herd is required.");
        if (resource.CorralId is null or <= 0) errors.Add("Corral is required.");
        if (resource.Quantity is < 1 or > 500) errors.Add("Quantity must be between 1 and 500.");
        if (resource.Weight < 0) errors.Add("Weight cannot be negative.");
        if (errors.Count > 0) return BadRequest(new { message = "One or more fields are invalid.", errors });
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeHerdId(resource.HerdId)) return this.Forbidden();

        var command = CreateAnimalBatchCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await commandService.Handle(command, cancellationToken);
        if (result.IsFailure) return BadRequest(new { message = result.Message });
        return Created(string.Empty, result.Value!.Select(AnimalResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>
    /// Changes the status of one animal. Sending "Archivado" archives it and any other status restores it.
    /// Rancher role only.
    /// </summary>
    [Authorize("Rancher")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateAnimalStatusResource resource, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.Status)) return BadRequest(new { message = "Status is required." });

        var existing = await queryService.Handle(new GetAnimalByIdQuery(id), cancellationToken);
        if (existing is null) return NotFound();
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeAnimalId(existing.Id)) return this.Forbidden();

        var result = await commandService.Handle(new UpdateAnimalsStatusCommand([id], resource.Status), cancellationToken);
        if (result.IsFailure) return BadRequest(new { message = result.Message });
        return Ok(AnimalResourceFromEntityAssembler.ToResourceFromEntity(result.Value!.Single()));
    }

    /// <summary>Updates the status of many animals at once. Rancher role only.</summary>
    [Authorize("Rancher")]
    [HttpPatch("bulk-status")]
    public async Task<IActionResult> UpdateBulkStatus(UpdateAnimalsStatusResource resource, CancellationToken cancellationToken)
    {
        if (resource.AnimalIds is null || resource.AnimalIds.Count == 0)
            return BadRequest(new { message = "At least one animal must be selected." });
        if (string.IsNullOrWhiteSpace(resource.Status)) return BadRequest(new { message = "Status is required." });
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (resource.AnimalIds.Any(animalId => !scope.CanSeeAnimalId(animalId))) return this.Forbidden();

        var command = new UpdateAnimalsStatusCommand(resource.AnimalIds, resource.Status);
        var result = await commandService.Handle(command, cancellationToken);
        if (result.IsFailure) return BadRequest(new { message = result.Message });
        return Ok(result.Value!.Select(AnimalResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>Deletes many animals by id list. Rancher role only.</summary>
    [Authorize("Rancher")]
    [HttpDelete("bulk")]
    public async Task<IActionResult> DeleteBatch(DeleteAnimalsResource resource, CancellationToken cancellationToken)
    {
        if (resource.AnimalIds is null || resource.AnimalIds.Count == 0)
            return BadRequest(new { message = "At least one animal must be selected." });
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (resource.AnimalIds.Any(animalId => !scope.CanSeeAnimalId(animalId))) return this.Forbidden();

        var result = await commandService.Handle(new DeleteAnimalsCommand(resource.AnimalIds), cancellationToken);
        if (result.IsFailure) return BadRequest(new { message = result.Message });
        return NoContent();
    }

    /// <summary>
    /// Uploads an animal image to wwwroot/uploads/animals and returns its public URL.
    /// Max size 10 MB; only JPEG, PNG, WEBP, or GIF. Rancher role only.
    /// </summary>
    [Authorize("Rancher")]
    [HttpPost("upload-image")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadImage(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0) return BadRequest(new { message = "An image file is required." });
        if (!AllowedImageContentTypes.Contains(file.ContentType))
            return BadRequest(new { message = "Only JPEG, PNG, WEBP or GIF images are allowed." });

        var webRootPath = webHostEnvironment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
            webRootPath = Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");

        var uploadsFolder = Path.Combine(webRootPath, "uploads", "animals");
        Directory.CreateDirectory(uploadsFolder);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        return Ok(new { url = $"/uploads/animals/{fileName}" });
    }
}
