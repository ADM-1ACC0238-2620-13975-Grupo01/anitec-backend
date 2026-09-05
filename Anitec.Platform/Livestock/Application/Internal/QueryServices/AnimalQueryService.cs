using Anitec.Platform.Livestock.Application.QueryServices;
using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Livestock.Domain.Model.Queries;
using Anitec.Platform.Livestock.Domain.Repositories;

namespace Anitec.Platform.Livestock.Application.Internal.QueryServices;

public class AnimalQueryService(IAnimalRepository repository) : IAnimalQueryService
{
    public async Task<Animal?> Handle(GetAnimalByIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }

    public async Task<IEnumerable<Animal>> Handle(GetAllAnimalsQuery query, CancellationToken cancellationToken)
    {
        return await repository.ListAsync(cancellationToken);
    }
}
