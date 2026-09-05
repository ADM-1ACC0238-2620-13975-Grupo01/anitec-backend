namespace Anitec.Platform.Analytics.Interfaces.Rest.Resources;

public record FinancialSummaryResource(decimal Income, decimal Expenses, decimal Balance, IEnumerable<MetricSliceResource> ByCategory);
