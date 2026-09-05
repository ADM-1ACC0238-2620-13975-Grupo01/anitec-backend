using System.Net.Mime;
using Anitec.Platform.Activities.Application.QueryServices;
using Anitec.Platform.Activities.Domain.Model.Queries;
using Anitec.Platform.Analytics.Interfaces.Rest.Resources;
using Anitec.Platform.Clients.Application.QueryServices;
using Anitec.Platform.Clients.Domain.Model.Queries;
using Anitec.Platform.Devices.Application.QueryServices;
using Anitec.Platform.Devices.Domain.Model.Queries;
using Anitec.Platform.Financial.Application.QueryServices;
using Anitec.Platform.Financial.Domain.Model.Queries;
using Anitec.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Anitec.Platform.Livestock.Application.QueryServices;
using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Livestock.Domain.Model.Queries;
using Anitec.Platform.Sanitary.Application.QueryServices;
using Anitec.Platform.Sanitary.Domain.Model.Queries;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Anitec.Platform.Analytics.Interfaces.Rest;

[Authorize("Rancher", "Veterinarian")]
[ApiController]
[Route("api/v1/analytics")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available calculated dashboard analytics endpoints")]
public class DashboardAnalyticsController(
    IHerdQueryService herdQueryService,
    IAnimalQueryService animalQueryService,
    IHealthEventQueryService healthEventQueryService,
    IFinancialRecordQueryService financialRecordQueryService,
    IFarmActivityQueryService farmActivityQueryService,
    IDeviceQueryService deviceQueryService,
    IVeterinarianClientQueryService veterinarianClientQueryService) : ControllerBase
{
    [HttpGet("ranchers/{rancherId:int}/dashboard")]
    public async Task<IActionResult> GetRancherDashboard(int rancherId, CancellationToken cancellationToken)
    {
        var herds = (await herdQueryService.Handle(new GetAllHerdsQuery(), cancellationToken))
            .Where(herd => herd.OwnerId == rancherId)
            .ToList();
        var animals = await GetAnimalsForHerds(herds, cancellationToken);
        var animalIds = animals.Select(animal => animal.Id).ToHashSet();
        var healthEvents = (await healthEventQueryService.Handle(new GetAllHealthEventsQuery(), cancellationToken))
            .Where(item => animalIds.Contains(item.AnimalId))
            .ToList();
        var records = (await financialRecordQueryService.Handle(new GetAllFinancialRecordsQuery(), cancellationToken))
            .Where(record => record.OwnerId == rancherId)
            .ToList();
        var activities = (await farmActivityQueryService.Handle(new GetAllFarmActivitiesQuery(), cancellationToken))
            .Where(activity => activity.OwnerId == rancherId)
            .ToList();
        var devices = (await deviceQueryService.Handle(new GetAllDevicesQuery(), cancellationToken))
            .Where(device => device.HerdId.HasValue && herds.Any(herd => herd.Id == device.HerdId.Value)
                             || device.AnimalId.HasValue && animalIds.Contains(device.AnimalId.Value))
            .ToList();

        var income = records
            .Where(record => record.Type.Equals("Income", StringComparison.OrdinalIgnoreCase)
                             || record.Type.Equals("Ingreso", StringComparison.OrdinalIgnoreCase))
            .Sum(record => record.Amount);
        var expenses = records
            .Where(record => record.Type.Equals("Expense", StringComparison.OrdinalIgnoreCase)
                             || record.Type.Equals("Egreso", StringComparison.OrdinalIgnoreCase))
            .Sum(record => record.Amount);

        return Ok(new RancherDashboardResource(
            herds.Count,
            animals.Count,
            animals.Count(animal => animal.Status.Equals("Healthy", StringComparison.OrdinalIgnoreCase)
                                    || animal.Status.Equals("Sano", StringComparison.OrdinalIgnoreCase)
                                    || animal.Status.Equals("Saludable", StringComparison.OrdinalIgnoreCase)),
            healthEvents.Count,
            activities.Count(activity => activity.Date >= DateOnly.FromDateTime(DateTime.UtcNow)),
            devices.Count,
            income,
            expenses,
            income - expenses));
    }

    [HttpGet("veterinarians/{veterinarianId:int}/dashboard")]
    public async Task<IActionResult> GetVeterinarianDashboard(int veterinarianId, CancellationToken cancellationToken)
    {
        var clients = (await veterinarianClientQueryService.Handle(
            new GetVeterinarianClientsByVeterinarianIdQuery(veterinarianId),
            cancellationToken)).ToList();
        var rancherIds = clients.Select(client => client.RancherId).ToHashSet();
        var herds = (await herdQueryService.Handle(new GetAllHerdsQuery(), cancellationToken))
            .Where(herd => rancherIds.Contains(herd.OwnerId) || herd.VeterinarianId == veterinarianId)
            .ToList();
        var animals = await GetAnimalsForHerds(herds, cancellationToken);
        var animalIds = animals.Select(animal => animal.Id).ToHashSet();
        var healthEvents = (await healthEventQueryService.Handle(new GetAllHealthEventsQuery(), cancellationToken))
            .Where(item => animalIds.Contains(item.AnimalId))
            .ToList();
        var activities = (await farmActivityQueryService.Handle(new GetAllFarmActivitiesQuery(), cancellationToken))
            .Where(activity => activity.VeterinarianId == veterinarianId)
            .ToList();

        return Ok(new VeterinarianDashboardResource(
            clients.Count,
            herds.Count,
            animals.Count,
            healthEvents.Count,
            activities.Count(activity => activity.Date >= DateOnly.FromDateTime(DateTime.UtcNow)),
            healthEvents.Count(item => !string.IsNullOrWhiteSpace(item.Treatment))));
    }

    [HttpGet("ranchers/{rancherId:int}/health-summary")]
    public async Task<IActionResult> GetRancherHealthSummary(int rancherId, CancellationToken cancellationToken)
    {
        var herds = (await herdQueryService.Handle(new GetAllHerdsQuery(), cancellationToken))
            .Where(herd => herd.OwnerId == rancherId)
            .ToList();
        var animals = await GetAnimalsForHerds(herds, cancellationToken);
        var animalIds = animals.Select(animal => animal.Id).ToHashSet();
        var events = (await healthEventQueryService.Handle(new GetAllHealthEventsQuery(), cancellationToken))
            .Where(item => animalIds.Contains(item.AnimalId))
            .ToList();

        return Ok(new HealthSummaryResource(
            events.Count,
            events
                .GroupBy(item => item.Type)
                .Select(group => new MetricSliceResource(group.Key, group.Count()))));
    }

    [HttpGet("ranchers/{rancherId:int}/financial-summary")]
    public async Task<IActionResult> GetRancherFinancialSummary(int rancherId, CancellationToken cancellationToken)
    {
        var records = (await financialRecordQueryService.Handle(new GetAllFinancialRecordsQuery(), cancellationToken))
            .Where(record => record.OwnerId == rancherId)
            .ToList();
        var income = records
            .Where(record => record.Type.Equals("Income", StringComparison.OrdinalIgnoreCase)
                             || record.Type.Equals("Ingreso", StringComparison.OrdinalIgnoreCase))
            .Sum(record => record.Amount);
        var expenses = records
            .Where(record => record.Type.Equals("Expense", StringComparison.OrdinalIgnoreCase)
                             || record.Type.Equals("Egreso", StringComparison.OrdinalIgnoreCase))
            .Sum(record => record.Amount);

        return Ok(new FinancialSummaryResource(
            income,
            expenses,
            income - expenses,
            records
                .GroupBy(record => record.Category)
                .Select(group => new MetricSliceResource(group.Key, group.Sum(record => record.Amount)))));
    }

    private async Task<List<Animal>> GetAnimalsForHerds(IEnumerable<Herd> herds, CancellationToken cancellationToken)
    {
        var herdIds = herds.Select(herd => herd.Id).ToHashSet();
        return (await animalQueryService.Handle(new GetAllAnimalsQuery(), cancellationToken))
            .Where(animal => herdIds.Contains(animal.HerdId))
            .ToList();
    }
}
