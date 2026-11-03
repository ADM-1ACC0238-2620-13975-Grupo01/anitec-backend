namespace Anitec.Platform.Analytics.Domain.Model.Commands;
// Registro (record) inmutable que encapsula los datos necesarios para crear una nueva métrica de informe,
// incluyendo la etiqueta descriptiva, el valor de la métrica y su tendencia o indicador de variación.
public record CreateReportMetricCommand(string Label, string Value, string Trend);
