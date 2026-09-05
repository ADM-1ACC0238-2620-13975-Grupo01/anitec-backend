using Anitec.Platform.Financial.Domain.Model.Commands;
using Anitec.Platform.Financial.Domain.Model;
using Anitec.Platform.Financial.Domain.Model.Entities;
using Anitec.Platform.Shared.Application.Model;

namespace Anitec.Platform.Financial.Application.CommandServices;

public interface IFinancialRecordCommandService
{
    Task<Result<FinancialRecord>> Handle(CreateFinancialRecordCommand command, CancellationToken cancellationToken);
    Task<Result<FinancialRecord>> Handle(UpdateFinancialRecordCommand command, CancellationToken cancellationToken);
    Task<Result> Handle(DeleteFinancialRecordCommand command, CancellationToken cancellationToken);
}
