using Anitec.Platform.Analytics.Domain.Model.Commands;

namespace Anitec.Platform.Analytics.Domain.Model.Entities;

public class ReportMetric
{
    public ReportMetric()
    {
        Label = string.Empty;
        Value = string.Empty;
        Trend = string.Empty;
    }

    public ReportMetric(CreateReportMetricCommand command)
    {
        Label = command.Label;
        Value = command.Value;
        Trend = command.Trend;

    }

    public int Id { get; set; }
    public string Label { get; set; }
    public string Value { get; set; }
    public string Trend { get; set; }
}
