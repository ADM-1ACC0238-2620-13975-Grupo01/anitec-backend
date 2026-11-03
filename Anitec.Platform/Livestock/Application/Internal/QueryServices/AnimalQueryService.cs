using Anitec.Platform.Livestock.Application.QueryServices;
using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Livestock.Domain.Model.Queries;
using Anitec.Platform.Livestock.Domain.Repositories;

namespace Anitec.Platform.Livestock.Application.Internal.QueryServices;

/// <summary>
/// Read-only queries over animals.
/// </summary>
public class AnimalQueryService(IAnimalRepository repository) : IAnimalQueryService
{
    /// <summary>Animal with the given id, or null.</summary>
    public async Task<Animal?> Handle(GetAnimalByIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }

    /// <summary>Every animal; filtering by user is done by the caller.</summary>
    public async Task<IEnumerable<Animal>> Handle(GetAllAnimalsQuery query, CancellationToken cancellationToken)
    {
        return await repository.ListAsync(cancellationToken);
    }
}
