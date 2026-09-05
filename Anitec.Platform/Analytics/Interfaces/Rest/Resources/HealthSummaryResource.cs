namespace Anitec.Platform.Analytics.Interfaces.Rest.Resources;

public record HealthSummaryResource(int TotalEvents, IEnumerable<MetricSliceResource> ByType);
