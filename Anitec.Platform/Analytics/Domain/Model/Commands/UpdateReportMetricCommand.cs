namespace Anitec.Platform.Analytics.Domain.Model.Commands;
// Registro (record) inmutable que encapsula el identificador y los datos actualizados para modificar una métrica de informe existente.
public record UpdateReportMetricCommand(int Id, string Label, string Value, string Trend);
