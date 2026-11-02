using Anitec.Platform.Analytics.Domain.Model.Commands;

namespace Anitec.Platform.Analytics.Domain.Model.Entities;

/// <summary>
/// A summary figure shown on the analytics dashboard, for example the number of registered animals.
/// </summary>
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
    /// <summary>Name of the metric shown on the dashboard card.</summary>
    public string Label { get; set; }
    /// <summary>Displayed value, stored as text so it can hold counts, amounts or percentages.</summary>
    public string Value { get; set; }
    /// <summary>Short note on how the metric is changing or where it comes from.</summary>
    public string Trend { get; set; }
}
