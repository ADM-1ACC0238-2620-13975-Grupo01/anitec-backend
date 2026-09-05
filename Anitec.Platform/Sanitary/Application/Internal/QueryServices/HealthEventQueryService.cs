using Anitec.Platform.Sanitary.Application.QueryServices;
using Anitec.Platform.Sanitary.Domain.Model.Entities;
using Anitec.Platform.Sanitary.Domain.Model.Queries;
using Anitec.Platform.Sanitary.Domain.Repositories;

namespace Anitec.Platform.Sanitary.Application.Internal.QueryServices;

public class HealthEventQueryService(IHealthEventRepository repository) : IHealthEventQueryService
{
    public async Task<HealthEvent?> Handle(GetHealthEventByIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }

    public async Task<IEnumerable<HealthEvent>> Handle(GetAllHealthEventsQuery query, CancellationToken cancellationToken)
    {
        return await repository.ListAsync(cancellationToken);
    }
}




