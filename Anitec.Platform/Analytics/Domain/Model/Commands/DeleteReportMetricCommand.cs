namespace Anitec.Platform.Analytics.Domain.Model.Commands;
// Registro (record) inmutable que encapsula el identificador único necesario para eliminar una métrica de informe existente.
public record DeleteReportMetricCommand(int Id);
