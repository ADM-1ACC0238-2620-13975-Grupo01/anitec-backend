using Anitec.Platform.Financial.Domain.Model.Entities;
using Anitec.Platform.Financial.Domain.Repositories;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

namespace Anitec.Platform.Financial.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class FinancialRecordRepository(AppDbContext context) : BaseRepository<FinancialRecord>(context), IFinancialRecordRepository
{
}
