using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Livestock.Domain.Model.Queries;

namespace Anitec.Platform.Livestock.Application.QueryServices;

public interface IHerdQueryService
{
    Task<Herd?> Handle(GetHerdByIdQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<Herd>> Handle(GetAllHerdsQuery query, CancellationToken cancellationToken);
}




