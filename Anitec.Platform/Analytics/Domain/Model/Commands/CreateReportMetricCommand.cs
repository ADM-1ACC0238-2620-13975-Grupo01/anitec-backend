namespace Anitec.Platform.Analytics.Domain.Model.Commands;

public record CreateReportMetricCommand(string Label, string Value, string Trend);
