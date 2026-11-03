using Anitec.Platform.Analytics.Domain.Model.Entities;
using Anitec.Platform.Analytics.Domain.Model.Queries;

namespace Anitec.Platform.Analytics.Application.QueryServices;
// Interfaz del servicio de consultas (Query Service) para gestionar métricas de informes,
// exponiendo métodos asíncronos para buscar por identificador único o listar todos los registros disponibles.
public interface IReportMetricQueryService
{
    Task<ReportMetric?> Handle(GetReportMetricByIdQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<ReportMetric>> Handle(GetAllReportMetricsQuery query, CancellationToken cancellationToken);
}




