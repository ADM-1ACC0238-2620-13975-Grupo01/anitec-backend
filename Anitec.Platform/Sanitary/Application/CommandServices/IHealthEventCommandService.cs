using Anitec.Platform.Sanitary.Domain.Model.Commands;
using Anitec.Platform.Sanitary.Domain.Model;
using Anitec.Platform.Sanitary.Domain.Model.Entities;
using Anitec.Platform.Shared.Application.Model;

namespace Anitec.Platform.Sanitary.Application.CommandServices;

public interface IHealthEventCommandService
{
    Task<Result<HealthEvent>> Handle(CreateHealthEventCommand command, CancellationToken cancellationToken);
    Task<Result<HealthEvent>> Handle(UpdateHealthEventCommand command, CancellationToken cancellationToken);
    Task<Result> Handle(DeleteHealthEventCommand command, CancellationToken cancellationToken);
}
