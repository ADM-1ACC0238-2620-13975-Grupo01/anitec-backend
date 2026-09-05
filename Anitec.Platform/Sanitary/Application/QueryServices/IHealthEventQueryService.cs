using Anitec.Platform.Sanitary.Domain.Model.Entities;
using Anitec.Platform.Sanitary.Domain.Model.Queries;

namespace Anitec.Platform.Sanitary.Application.QueryServices;

public interface IHealthEventQueryService
{
    Task<HealthEvent?> Handle(GetHealthEventByIdQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<HealthEvent>> Handle(GetAllHealthEventsQuery query, CancellationToken cancellationToken);
}




