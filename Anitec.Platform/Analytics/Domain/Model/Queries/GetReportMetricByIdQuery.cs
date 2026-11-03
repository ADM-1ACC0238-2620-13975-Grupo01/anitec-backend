namespace Anitec.Platform.Analytics.Domain.Model.Queries;
// Registro (record) inmutable que encapsula el identificador único necesario para consultar una métrica de informe específica.
public record GetReportMetricByIdQuery(int Id);
