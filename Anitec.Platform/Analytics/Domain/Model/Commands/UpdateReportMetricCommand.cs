namespace Anitec.Platform.Analytics.Domain.Model.Commands;

public record UpdateReportMetricCommand(int Id, string Label, string Value, string Trend);
