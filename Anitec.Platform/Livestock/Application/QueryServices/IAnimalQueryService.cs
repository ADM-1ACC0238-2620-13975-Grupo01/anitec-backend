using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Livestock.Domain.Model.Queries;

namespace Anitec.Platform.Livestock.Application.QueryServices;

public interface IAnimalQueryService
{
    Task<Animal?> Handle(GetAnimalByIdQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<Animal>> Handle(GetAllAnimalsQuery query, CancellationToken cancellationToken);
}




