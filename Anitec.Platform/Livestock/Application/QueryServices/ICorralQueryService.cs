using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Livestock.Domain.Model.Queries;

namespace Anitec.Platform.Livestock.Application.QueryServices;

public interface ICorralQueryService
{
    Task<Corral?> Handle(GetCorralByIdQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<Corral>> Handle(GetAllCorralsQuery query, CancellationToken cancellationToken);
}
